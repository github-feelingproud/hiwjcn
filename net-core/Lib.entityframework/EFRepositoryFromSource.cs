using Lib.data;
using Lib.data.ef;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Lib.entityframework
{
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
}
