using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.data
{
    public interface ILinqRepository<T>
    {
        #region 获取查询上下文
        /// <summary>
        /// 获取IQueryable对象，用于linq查询
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="track"></param>
        void PrepareIQueryable(Func<IQueryable<T>, bool> callback, bool track = false);

        /// <summary>
        /// 获取IQueryable对象，用于linq查询
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="track"></param>
        /// <returns></returns>
        Task PrepareIQueryableAsync(Func<IQueryable<T>, Task<bool>> callback, bool track = false);

        #endregion

        #region 不用返回true的查询上下文

        /// <summary>
        /// 不用return true
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="track"></param>
        void PrepareIQueryable(Action<IQueryable<T>> callback, bool track = false);

        /// <summary>
        /// 不用return true
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="track"></param>
        /// <returns></returns>
        Task PrepareIQueryableAsync(Func<IQueryable<T>, Task> callback, bool track = false);

        #endregion

        #region 可以直接返回查询结果

        /// <summary>
        /// 可以拿到返回值
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="callback"></param>
        /// <param name="track"></param>
        /// <returns></returns>
        R PrepareIQueryable_<R>(Func<IQueryable<T>, R> callback, bool track = false);

        /// <summary>
        /// 可以拿到返回值
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <param name="callback"></param>
        /// <param name="track"></param>
        /// <returns></returns>
        Task<R> PrepareIQueryableAsync_<R>(Func<IQueryable<T>, Task<R>> callback, bool track = false);

        #endregion
    }
}
