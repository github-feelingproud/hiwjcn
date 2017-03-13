using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Lib.data
{
    /// <summary>
    /// 仓储接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T> where T : DBTable
    {
        /// <summary>
        /// 数据源选择
        /// </summary>
        EFManager _EFManager { get; }

        /// <summary>
        /// 插入
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        int Add(params T[] models);
        /// <summary>
        /// 异步插入
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        Task<int> AddAsync(params T[] models);
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        int Delete(params T[] models);
        /// <summary>
        /// 异步删除
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        Task<int> DeleteAsync(params T[] models);
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        int Update(params T[] models);
        /// <summary>
        /// 异步更新
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        Task<int> UpdateAsync(params T[] models);
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        T GetByKeys(params object[] keys);
        /// <summary>
        /// 异步查询
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        Task<T> GetByKeysAsync(params object[] keys);
        /// <summary>
        /// 查询
        /// </summary>
        /// <typeparam name="OrderByColumnType"></typeparam>
        /// <param name="where"></param>
        /// <param name="orderby"></param>
        /// <param name="Desc"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<T> QueryList<OrderByColumnType>(
            Expression<Func<T, bool>> where,
            Expression<Func<T, OrderByColumnType>> orderby = null,
            bool Desc = true,
            int start = default(int),
            int count = default(int));
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="where"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<T> GetList(Expression<Func<T, bool>> where, int count = default(int));
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        T GetFirst(Expression<Func<T, bool>> where);
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        int GetCount(Expression<Func<T, bool>> where);
        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        bool Exist(Expression<Func<T, bool>> where);
        /// <summary>
        /// 获取ef对象
        /// </summary>
        /// <param name="callback"></param>
        void PrepareSession(Func<DbContext, bool> callback);
        /// <summary>
        /// 获取queryable
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="track"></param>
        void PrepareIQueryable(Func<IQueryable<T>, bool> callback, bool track = true);
    }
}
