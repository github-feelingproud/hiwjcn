using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.data
{
    public interface ILinqRepository<T>
    {
        /// <summary>
        /// 不用return true
        /// </summary>
        void PrepareIQueryable(Action<IQueryable<T>> callback, bool track = false);

        /// <summary>
        /// 不用return true
        /// </summary>
        Task PrepareIQueryableAsync(Func<IQueryable<T>, Task> callback, bool track = false);

        /// <summary>
        /// 可以拿到返回值
        /// </summary>
        R PrepareIQueryable<R>(Func<IQueryable<T>, R> callback, bool track = false);

        /// <summary>
        /// 可以拿到返回值
        /// </summary>
        Task<R> PrepareIQueryableAsync<R>(Func<IQueryable<T>, Task<R>> callback, bool track = false);

    }
}
