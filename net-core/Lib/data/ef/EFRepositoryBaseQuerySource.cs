using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lib.data.ef
{
    public abstract partial class EFRepositoryBase<T> : IEFRepository<T>
        where T : class, IDBTable
    {
        /// <summary>
        /// 让子类实现
        /// </summary>
        /// <param name="callback"></param>
        public abstract void PrepareSession(Action<DbContext> callback);

        /// <summary>
        /// 让子类实现
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public abstract Task PrepareSessionAsync(Func<DbContext, Task> callback);

        /*----------------------------------------------------------------------------*/

        public R PrepareSession<R>(Func<DbContext, R> callback)
        {
            var data = default(R);
            this.PrepareSession(db =>
            {
                data = callback.Invoke(db);
            });
            return data;
        }

        public async Task<R> PrepareSessionAsync<R>(Func<DbContext, Task<R>> callback)
        {
            var data = default(R);
            await this.PrepareSessionAsync(async db =>
            {
                data = await callback.Invoke(db);
            });
            return data;
        }

        /*----------------------------------------------------------------------------*/

        public virtual void PrepareIQueryable(Action<IQueryable<T>> callback)
        {
            this.PrepareSession(db =>
            {
                var query = db.Set<T>().AsNoTrackingQueryable();
                callback.Invoke(query);
            });
        }

        public virtual async Task PrepareIQueryableAsync(Func<IQueryable<T>, Task> callback)
        {
            await this.PrepareSessionAsync(async db =>
            {
                var query = db.Set<T>().AsNoTrackingQueryable();
                await callback.Invoke(query);
            });
        }

        /*----------------------------------------------------------------------------*/

        public R PrepareIQueryable<R>(Func<IQueryable<T>, R> callback)
        {
            return this.PrepareSession(db =>
            {
                var query = db.Set<T>().AsNoTrackingQueryable();
                return callback.Invoke(query);
            });
        }

        public async Task<R> PrepareIQueryableAsync<R>(Func<IQueryable<T>, Task<R>> callback)
        {
            return await this.PrepareSessionAsync(async db =>
            {
                var query = db.Set<T>().AsNoTrackingQueryable();
                return await callback.Invoke(query);
            });
        }

    }
}
