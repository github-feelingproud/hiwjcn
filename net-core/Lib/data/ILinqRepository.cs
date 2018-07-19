using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lib.data
{
    public interface ILinqRepository<T>
    {
        /// <summary>
        /// 不用return true
        /// </summary>
        void PrepareIQueryable(Action<IQueryable<T>> callback);

        /// <summary>
        /// 不用return true
        /// </summary>
        Task PrepareIQueryableAsync(Func<IQueryable<T>, Task> callback);

        /// <summary>
        /// 可以拿到返回值
        /// </summary>
        R PrepareIQueryable<R>(Func<IQueryable<T>, R> callback);

        /// <summary>
        /// 可以拿到返回值
        /// </summary>
        Task<R> PrepareIQueryableAsync<R>(Func<IQueryable<T>, Task<R>> callback);

    }
}
