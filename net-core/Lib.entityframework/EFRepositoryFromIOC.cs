using Lib.data;
using Lib.data.ef;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Lib.entityframework
{
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
