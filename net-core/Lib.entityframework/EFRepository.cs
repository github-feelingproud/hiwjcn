using Lib.entityframework;
using Lib.ioc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lib.data.ef
{
    /// <summary>
    /// 使用依赖注入中name为db的dbcontext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EFRepository<T> : EFRepositoryBase<T> where T : class, IDBTable
    {
        #region 获取查询上下文

        public override void PrepareSession(Action<DbContext> callback)
        {
            using (var s = IocContext.Instance.Scope())
            {
                var context = s.ResolveAll_<IServiceWrapper<DbContext>>().FirstOrDefault(x => x.Name == Bootstrap.DefaultName);
                context = context ?? throw new NotRegException("ef dbcontext not registed");
                using (var con = context.Value)
                {
                    callback.Invoke(con);
                }
            }
        }
        public override async Task PrepareSessionAsync(Func<DbContext, Task> callback)
        {
            using (var s = IocContext.Instance.Scope())
            {
                var context = s.ResolveAll_<IServiceWrapper<DbContext>>().FirstOrDefault(x => x.Name == Bootstrap.DefaultName);
                context = context ?? throw new NotRegException("ef dbcontext not registed");
                using (var con = context.Value)
                {
                    await callback.Invoke(con);
                }
            }
        }

        #endregion
    }
}
