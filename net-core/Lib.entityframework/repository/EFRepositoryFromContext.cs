using Lib.data;
using Lib.data.ef;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Lib.entityframework
{
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
}
