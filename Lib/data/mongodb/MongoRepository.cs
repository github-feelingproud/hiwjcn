using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Lib.data.mongodb
{
    /*
                var mongo = new MongoClient("mongodb://127.0.0.1:27017");
                var db = mongo.GetDatabase("hiwjcn");
                var users = db.GetCollection<Users>("users");

                var delete = users.DeleteMany(x => true);

                users.InsertMany(Com.Range(5000).Select(x => new Users() { Name = $"user:{x}" }));

                var query = users.AsQueryable();
                var list = query.Take(500).ToList();

                list = query.Where(x => x.Name.Contains("1")).ToList();
         */

    public class MongoRepository<T> : IRepository<T>
        where T : MongoEntityBase
    {
        public int Add(params T[] models)
        {
            throw new NotImplementedException();
        }

        public Task<int> AddAsync(params T[] models)
        {
            throw new NotImplementedException();
        }

        public int Delete(params T[] models)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteAsync(params T[] models)
        {
            throw new NotImplementedException();
        }

        public int DeleteWhere(Expression<Func<T, bool>> where)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteWhereAsync(Expression<Func<T, bool>> where)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool Exist(Expression<Func<T, bool>> where)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistAsync(Expression<Func<T, bool>> where)
        {
            throw new NotImplementedException();
        }

        public T GetByKeys(params object[] keys)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetByKeysAsync(params object[] keys)
        {
            throw new NotImplementedException();
        }

        public int GetCount(Expression<Func<T, bool>> where)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCountAsync(Expression<Func<T, bool>> where)
        {
            throw new NotImplementedException();
        }

        public T GetFirst(Expression<Func<T, bool>> where)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetFirstAsync(Expression<Func<T, bool>> where)
        {
            throw new NotImplementedException();
        }

        public List<T> GetList(Expression<Func<T, bool>> where, int? count = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> GetListAsync(Expression<Func<T, bool>> where, int? count = null)
        {
            throw new NotImplementedException();
        }

        public List<T> GetListEnsureMaxCount(Expression<Func<T, bool>> where, int count, string error_msg)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> GetListEnsureMaxCountAsync(Expression<Func<T, bool>> where, int count, string error_msg)
        {
            throw new NotImplementedException();
        }

        public void PrepareIQueryable(Func<IQueryable<T>, bool> callback, bool track = false)
        {
            throw new NotImplementedException();
        }

        public void PrepareIQueryable(Action<IQueryable<T>> callback, bool track = false)
        {
            throw new NotImplementedException();
        }

        public Task PrepareIQueryableAsync(Func<IQueryable<T>, Task<bool>> callback, bool track = false)
        {
            throw new NotImplementedException();
        }

        public Task PrepareIQueryableAsync(Func<IQueryable<T>, Task> callback, bool track = false)
        {
            throw new NotImplementedException();
        }

        public Task<R> PrepareIQueryableAsync_<R>(Func<IQueryable<T>, Task<R>> callback, bool track = false)
        {
            throw new NotImplementedException();
        }

        public R PrepareIQueryable_<R>(Func<IQueryable<T>, R> callback, bool track = false)
        {
            throw new NotImplementedException();
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
