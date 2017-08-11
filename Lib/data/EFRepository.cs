using Lib.extension;
using Lib.helper;
using Lib.infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Lib.data
{
    /// <summary>
    /// 标准sql中使用groupby需要有聚合函数（mysql除外），所以没有封装。
    /// 如果使用groupby查询请手写session或者iqueryable查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class EFRepository<T> : EFRepositoryBase<T> where T : class, IDBTable
    {
        public EFManager _EFManager { get; private set; }

        public EFRepository() : this("db") { }
        public EFRepository(string db_name)
        {
            this._EFManager = new EFManager(db_name);
        }

        #region 获取查询上下文

        public override void PrepareIQueryable(Func<IQueryable<T>, bool> callback, bool track = false)
        {
            this._EFManager.PrepareIQueryable<T>(callback, track);
        }
        public override async Task PrepareIQueryableAsync(Func<IQueryable<T>, Task<bool>> callback, bool track = false)
        {
            await this._EFManager.PrepareIQueryableAsync<T>(callback, track);
        }

        public override void PrepareSession(Func<DbContext, bool> callback)
        {
            this._EFManager.PrepareSession(callback);
        }
        public override async Task PrepareSessionAsync(Func<DbContext, Task<bool>> callback)
        {
            await this._EFManager.PrepareSessionAsync(callback);
        }

        #endregion
    }

    partial class EFRepository<T>
    {
        public void TODO_PLACE_ASYNC_METHOD_IN_THIS_PARTIAL_CLASS() { }
    }

    public class EFRepositoryFromSource<T> : EFRepositoryBase<T> where T : class, IDBTable
    {
        private readonly Func<DbContext> GetContext;

        public EFRepositoryFromSource(Func<DbContext> GetContext)
        {
            this.GetContext = GetContext ?? throw new ArgumentNullException(nameof(GetContext));
        }

        public override void PrepareIQueryable(Func<IQueryable<T>, bool> callback, bool track = false)
        {
            using (var db = this.GetContext.Invoke())
            {
                var query = db.Set<T>().AsQueryableTrackingOrNot(track);
                callback.Invoke(query);
            }
        }

        public override async Task PrepareIQueryableAsync(Func<IQueryable<T>, Task<bool>> callback, bool track = false)
        {
            using (var db = this.GetContext.Invoke())
            {
                var query = db.Set<T>().AsQueryableTrackingOrNot(track);
                await callback.Invoke(query);
            }
        }

        public override void PrepareSession(Func<DbContext, bool> callback)
        {
            using (var db = this.GetContext.Invoke())
            {
                callback.Invoke(db);
            }
        }

        public override async Task PrepareSessionAsync(Func<DbContext, Task<bool>> callback)
        {
            using (var db = this.GetContext.Invoke())
            {
                await callback.Invoke(db);
            }
        }
    }
}
