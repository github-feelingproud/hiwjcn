using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Lib.ioc;

namespace Lib.data.ef
{
    /// <summary>
    /// 使用依赖注入中name为db的dbcontext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EFRepository<T> : EFRepositoryBase<T> where T : class, IDBTable
    {
        public EFRepository() { }

        #region 获取查询上下文

        public override void PrepareSession(Action<DbContext> callback)
        {
            using (var s = IocContext.Instance.Scope())
            {
                using (var con = s.Resolve_<DbContext>())
                {
                    callback.Invoke(con);
                }
            }
        }
        public override async Task PrepareSessionAsync(Func<DbContext, Task> callback)
        {
            using (var s = IocContext.Instance.Scope())
            {
                using (var con = s.Resolve_<DbContext>())
                {
                    await callback.Invoke(con);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 手动传入dbcontext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EFRepositoryFromSource<T> : EFRepositoryBase<T> where T : class, IDBTable
    {
        private readonly Func<DbContext> GetContext;

        public EFRepositoryFromSource(Func<DbContext> GetContext)
        {
            this.GetContext = GetContext ?? throw new ArgumentNullException(nameof(GetContext));
        }

        public override void PrepareSession(Action<DbContext> callback)
        {
            using (var db = this.GetContext.Invoke())
            {
                callback.Invoke(db);
            }
        }

        public override async Task PrepareSessionAsync(Func<DbContext, Task> callback)
        {
            using (var db = this.GetContext.Invoke())
            {
                await callback.Invoke(db);
            }
        }
    }

    /// <summary>
    /// 通过泛型指定dbcontext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Context"></typeparam>
    public class EFRepositoryFromContext<T, Context> : EFRepositoryBase<T>
        where T : class, IDBTable
        where Context : DbContext, new()
    {
        public override void PrepareSession(Action<DbContext> callback)
        {
            using (var db = new Context())
            {
                callback.Invoke(db);
            }
        }

        public override async Task PrepareSessionAsync(Func<DbContext, Task> callback)
        {
            using (var db = new Context())
            {
                await callback.Invoke(db);
            }
        }
    }

    /// <summary>
    /// 从依赖注入中获取默认dbcontext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EFRepositoryFromIOC<T> : EFRepositoryBase<T>
        where T : class, IDBTable
    {
        private readonly DbContext _context;

        public EFRepositoryFromIOC(DbContext _context)
        {
            this._context = _context;
        }

        public override void PrepareSession(Action<DbContext> callback)
        {
            callback.Invoke(this._context);
        }

        public override async Task PrepareSessionAsync(Func<DbContext, Task> callback)
        {
            await callback.Invoke(this._context);
        }

        public override void Dispose()
        {
            this._context?.Dispose();
        }
    }
}
