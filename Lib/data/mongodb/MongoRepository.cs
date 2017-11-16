using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using Lib.extension;
using Lib.data.ef;

namespace Lib.data.mongodb
{
    public class MongoRepository<T, Context>
        where T : MongoEntityBase
        where Context : MongoContextBase, new()
    {
        private IMongoCollection<T> Set() => new Context().Set<T>();

        public int Add(params T[] models)
        {
            this.Set().InsertMany(models);
            return models.Count();
        }

        public async Task<int> AddAsync(params T[] models)
        {
            await this.Set().InsertManyAsync(models);
            return models.Count();
        }

        public int Delete(params T[] models)
        {
            var uids = models.Select(x => x._id);
            var filter = Builders<T>.Filter.Where(x => uids.Contains(x._id));
            return (int)this.Set().DeleteMany(filter).DeletedCount;
        }

        public async Task<int> DeleteAsync(params T[] models)
        {
            var uids = models.Select(x => x._id);
            var filter = Builders<T>.Filter.Where(x => uids.Contains(x._id));
            return (int)(await this.Set().DeleteManyAsync(filter)).DeletedCount;
        }

        public int DeleteWhere(Expression<Func<T, bool>> where)
        {
            return (int)this.Set().DeleteMany(where).DeletedCount;
        }

        public async Task<int> DeleteWhereAsync(Expression<Func<T, bool>> where)
        {
            return (int)(await this.Set().DeleteManyAsync(where)).DeletedCount;
        }

        public void Dispose()
        {
            //
        }

        public bool Exist(Expression<Func<T, bool>> where)
        {
            return this.Set().Find(where).Limit(1).FirstOrDefault() != null;
        }

        public async Task<bool> ExistAsync(Expression<Func<T, bool>> where)
        {
            return (await this.Set().FindAsync(where)).FirstOrDefault() != null;
        }

        public int GetCount(Expression<Func<T, bool>> where)
        {
            return (int)this.Set().Count(where);
        }

        public async Task<int> GetCountAsync(Expression<Func<T, bool>> where)
        {
            return (int)(await this.Set().CountAsync(where));
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

        public List<T> GetListEnsureMaxCount(Expression<Func<T, bool>> where, int count, string error_msg)
        {
            var list = this.GetList(where, count);
            if (list.Count >= count) { throw new Exception(error_msg); }
            return list;
        }

        public async Task<List<T>> GetListEnsureMaxCountAsync(Expression<Func<T, bool>> where, int count, string error_msg)
        {
            var list = await this.GetListAsync(where, count);
            if (list.Count >= count) { throw new Exception(error_msg); }
            return list;
        }

        public void PrepareIQueryable(Func<IQueryable<T>, bool> callback, bool track = false)
        {
            var q = this.Set().AsQueryable().AsQueryableTrackingOrNot(track);
            callback.Invoke(q);
        }

        public void PrepareIQueryable(Action<IQueryable<T>> callback, bool track = false)
        {
            var q = this.Set().AsQueryable().AsQueryableTrackingOrNot(track);
            callback.Invoke(q);
        }

        public async Task PrepareIQueryableAsync(Func<IQueryable<T>, Task<bool>> callback, bool track = false)
        {
            var q = this.Set().AsQueryable().AsQueryableTrackingOrNot(track);
            await callback.Invoke(q);
        }

        public async Task PrepareIQueryableAsync(Func<IQueryable<T>, Task> callback, bool track = false)
        {
            var q = this.Set().AsQueryable().AsQueryableTrackingOrNot(track);
            await callback.Invoke(q);
        }

        public async Task<R> PrepareIQueryableAsync_<R>(Func<IQueryable<T>, Task<R>> callback, bool track = false)
        {
            var q = this.Set().AsQueryable().AsQueryableTrackingOrNot(track);
            return await callback.Invoke(q);
        }

        public R PrepareIQueryable_<R>(Func<IQueryable<T>, R> callback, bool track = false)
        {
            var q = this.Set().AsQueryable().AsQueryableTrackingOrNot(track);
            return callback.Invoke(q);
        }

        public void PrepareSession(Func<DbContext, bool> callback)
        {
            throw new NotImplementedException();
        }

        public void PrepareSession(Action<DbContext> callback)
        {
            throw new NotImplementedException();
        }

        public Task PrepareSessionAsync(Func<DbContext, Task<bool>> callback)
        {
            throw new NotImplementedException();
        }

        public Task PrepareSessionAsync(Func<DbContext, Task> callback)
        {
            throw new NotImplementedException();
        }

        public Task<R> PrepareSessionAsync_<R>(Func<DbContext, Task<R>> callback)
        {
            throw new NotImplementedException();
        }

        public R PrepareSession_<R>(Func<DbContext, R> callback)
        {
            throw new NotImplementedException();
        }

        public List<T> QueryList<OrderByColumnType>(Expression<Func<T, bool>> where, Expression<Func<T, OrderByColumnType>> orderby = null, bool Desc = true, int? start = null, int? count = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> QueryListAsync<OrderByColumnType>(Expression<Func<T, bool>> where, Expression<Func<T, OrderByColumnType>> orderby = null, bool Desc = true, int? start = null, int? count = null)
        {
            throw new NotImplementedException();
        }

        public int Update(params T[] models)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateAsync(params T[] models)
        {
            throw new NotImplementedException();
        }
    }
}
