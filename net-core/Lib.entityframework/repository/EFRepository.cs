using Lib.ioc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Lib.data.ef
{
    /// <summary>
    /// 使用依赖注入中name为db的dbcontext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EFRepository<T> : EFRepositoryBase<T>
        where T : class, IDBTable
    {
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
    }
}
