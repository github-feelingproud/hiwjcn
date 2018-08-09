using Lib.extension;
using Lib.helper;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Lib.mongodb
{
    public class MongoRepository<T> : IMongoRepository<T>
        where T : MongoEntityBase
    {
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _db;
        private readonly IMongoCollection<T> _set;

        public MongoRepository(IMongoClientWrapper wrapper)
        {
            this._client = wrapper.Value;

            this._db = this._client.GetDatabase(wrapper.DatabaseName);
            this._set = this._db.GetCollection<T>(typeof(T).GetTableName());
        }

        [Obsolete]
        public void test()
        {
            var set = this._set;

            //map reduce
            set.MapReduce<_>(
                new BsonJavaScript("function(){emit(this.user_id,this.age);}"),
                new BsonJavaScript("function(user_id,age){return Array.avg(age);}"),
                new MapReduceOptions<T, _>() { });

            //geo index
            var index = Builders<T>.IndexKeys.Geo2D(x => x._id).Geo2DSphere(x => x._id);
            set.Indexes.CreateOne(index, new CreateIndexOptions() { Unique = true, Background = true });

            //agg
            var filter = Builders<T>.Filter.Where(x => x._id == null);
            var group = Builders<T>.Projection.Exclude(x => x._id).Include(x => x._id);
            var agg = set.Aggregate().Match(filter).Group(group).SortByCount(x => x.AsObjectId).ToList();
        }

        public List<T> QueryNearBy(Expression<Func<T, bool>> where, int page, int pagesize,
            Expression<Func<T, object>> field, GeoInfo point,
            double? max_distance = null, double? min_distance = null)
        {
            var condition = Builders<T>.Filter.Empty;
            condition &= Builders<T>.Filter.Near(field, point.Lat, point.Lon, max_distance, min_distance);
            if (where != null)
            {
                condition &= Builders<T>.Filter.Where(where);
            }
            var range = PagerHelper.GetQueryRange(page, pagesize);
            return this._set.Find(condition).QueryPage(page, pagesize).ToList();
        }

        public int Add(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models))
                throw new ArgumentNullException(nameof(models));

            this._set.InsertMany(models);
            return models.Count();
        }

        public async Task<int> AddAsync(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models))
                throw new ArgumentNullException(nameof(models));

            await this._set.InsertManyAsync(models);
            return models.Count();
        }

        public int Delete(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models))
                throw new ArgumentNullException(nameof(models));

            var uids = models.Select(x => x._id);
            var filter = Builders<T>.Filter.Where(x => uids.Contains(x._id));
            return (int)this._set.DeleteMany(filter).DeletedCount;
        }

        public async Task<int> DeleteAsync(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models))
                throw new ArgumentNullException(nameof(models));

            var uids = models.Select(x => x._id);
            var filter = Builders<T>.Filter.Where(x => uids.Contains(x._id));
            return (int)(await this._set.DeleteManyAsync(filter)).DeletedCount;
        }

        public int DeleteWhere(Expression<Func<T, bool>> where)
        {
            where = where ?? throw new ArgumentNullException(nameof(where));

            return (int)this._set.DeleteMany(where).DeletedCount;
        }

        public async Task<int> DeleteWhereAsync(Expression<Func<T, bool>> where)
        {
            where = where ?? throw new ArgumentNullException(nameof(where));

            return (int)(await this._set.DeleteManyAsync(where)).DeletedCount;
        }

        public bool Exist(Expression<Func<T, bool>> where)
        {
            return this._set.Find(where).Take(1).FirstOrDefault() != null;
        }

        public async Task<bool> ExistAsync(Expression<Func<T, bool>> where)
        {
            return (await this._set.FindAsync(where)).FirstOrDefault() != null;
        }

        public int GetCount(Expression<Func<T, bool>> where)
        {
            return (int)this._set.CountDocuments(where);
        }

        public async Task<int> GetCountAsync(Expression<Func<T, bool>> where)
        {
            return (int)(await this._set.CountDocumentsAsync(where));
        }

        public T GetFirst(Expression<Func<T, bool>> where)
        {
            return this.GetList(where, 1).FirstOrDefault();
        }

        public async Task<T> GetFirstAsync(Expression<Func<T, bool>> where)
        {
            return (await this.GetListAsync(where, 1)).FirstOrDefault();
        }

        public List<T> GetList(Expression<Func<T, bool>> where, int? count = null)
        {
            return this.QueryList<object>(where: where, start: 0, count: count);
        }

        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> where, int? count = null)
        {
            return await this.QueryListAsync<object>(where: where, start: 0, count: count);
        }

        public void PrepareIQueryable(Action<IQueryable<T>> callback)
        {
            var q = this._set.AsQueryable();
            callback.Invoke(q);
        }

        public async Task PrepareIQueryableAsync(Func<IQueryable<T>, Task> callback)
        {
            var q = this._set.AsQueryable();
            await callback.Invoke(q);
        }

        public async Task<R> PrepareIQueryableAsync<R>(Func<IQueryable<T>, Task<R>> callback)
        {
            var q = this._set.AsQueryable();
            return await callback.Invoke(q);
        }

        public R PrepareIQueryable<R>(Func<IQueryable<T>, R> callback)
        {
            var q = this._set.AsQueryable();
            return callback.Invoke(q);
        }

        public List<T> QueryList<OrderByColumnType>(Expression<Func<T, bool>> where,
            Expression<Func<T, OrderByColumnType>> orderby = null, bool Desc = true,
            int? start = null, int? count = null)
        {
            var condition = Builders<T>.Filter.Empty;
            if (where != null)
            {
                condition &= where;
            }

            var query = this._set.Find(condition);
            if (orderby != null)
            {
                var sort = Builders<T>.Sort.Sort_(orderby, Desc);
                query = query.Sort(sort);
            }

            if (start != null)
            {
                query = query.Skip(start.Value);
            }
            if (count != null)
            {
                query = query.Take(count.Value);
            }
            return query.ToList();
        }

        public async Task<List<T>> QueryListAsync<OrderByColumnType>(Expression<Func<T, bool>> where, Expression<Func<T, OrderByColumnType>> orderby = null, bool Desc = true, int? start = null, int? count = null)
        {
            var condition = Builders<T>.Filter.Empty;
            if (where != null)
            {
                condition &= where;
            }

            var query = this._set.Find(condition);
            if (orderby != null)
            {
                var sort = Builders<T>.Sort.Sort_(orderby, Desc);
                query = query.Sort(sort);
            }

            if (start != null)
            {
                query = query.Skip(start.Value);
            }
            if (count != null)
            {
                query = query.Take(count.Value);
            }
            return await query.ToListAsync();
        }

        public int Update(params T[] models)
        {
            var set = this._set;
            return (int)models.Select(x => set.ReplaceOne(m => m._id == x._id, x).ModifiedCount).Sum();
        }

        public async Task<int> UpdateAsync(params T[] models)
        {
            var set = this._set;
            var res = await Task.WhenAll(models.Select(x => set.ReplaceOneAsync(m => m._id == x._id, x)));
            return (int)res.Select(x => x.ModifiedCount).Sum();
        }

        public void PrepareMongoCollection(Action<IMongoCollection<T>> callback)
        {
            callback.Invoke(this._set);
        }

        public async Task PrepareMongoCollectionAsync(Func<IMongoCollection<T>, Task> callback)
        {
            await callback.Invoke(this._set);
        }

        public R PrepareMongoCollection<R>(Func<IMongoCollection<T>, R> callback)
        {
            return callback.Invoke(this._set);
        }

        public async Task<R> PrepareMongoCollectionAsync<R>(Func<IMongoCollection<T>, Task<R>> callback)
        {
            return await callback.Invoke(this._set);
        }

        private ObjectId ParseID(params object[] keys)
        {
            var pid = keys?.FirstOrDefault()?.ToString();

            if (!ValidateHelper.IsPlumpString(pid))
                throw new ArgumentNullException("id不能为空");

            var id = new ObjectId(pid);
            return id;
        }

        public T GetByKeys(params object[] keys)
        {
            var id = ParseID(keys);
            return this._set.Find(x => x._id == id).FirstOrDefault();
        }

        public async Task<T> GetByKeysAsync(params object[] keys)
        {
            var id = ParseID(keys);
            return await this._set.Find(x => x._id == id).FirstOrDefaultAsync();
        }

        public virtual void Dispose() { }
    }
}
