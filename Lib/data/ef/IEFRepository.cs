using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.data.ef
{
    public interface IEFRepository<T>
    {
        /// <summary>
        /// 获取session
        /// </summary>
        void PrepareSession(Func<DbContext, bool> callback);

        /// <summary>
        /// 获取session
        /// </summary>
        Task PrepareSessionAsync(Func<DbContext, Task<bool>> callback);
    }
}
