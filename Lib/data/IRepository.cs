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
    public interface IRepository<T> where T : IDBTable
    {
        #region 添加
        /// <summary>
        /// 添加多个model
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        int Add(params T[] models);
        /// <summary>
        /// 异步添加
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        Task<int> AddAsync(params T[] models);
        #endregion

        #region 删除
        /// <summary>
        /// 删除多个model
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
        #endregion

        #region 修改
        /// <summary>
        /// 更新多个model
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

        #endregion

        #region 查询
        /// <summary>
        /// 根据主键查询实体
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        T GetByKeys(params object[] keys);

        /// <summary>
        /// 异步查找
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        Task<T> GetByKeysAsync(params object[] keys);

        /// <summary>
        /// 获取list
        /// expression和func的使用注意点，参见lib的readme
        /// </summary>
        /// <param name="where">where条件</param>
        /// <param name="orderby">排序条件</param>
        /// <param name="start">开始位置</param>
        /// <param name="count">读取条数</param>
        /// <param name="Desc">正序反序</param>
        /// <returns></returns>
        List<T> QueryList<OrderByColumnType>(
            Expression<Func<T, bool>> where,
            Expression<Func<T, OrderByColumnType>> orderby = null,
            bool Desc = true,
            int start = default(int),
            int count = default(int));

        Task<List<T>> QueryListAsync<OrderByColumnType>(
            Expression<Func<T, bool>> where,
            Expression<Func<T, OrderByColumnType>> orderby = null,
            bool Desc = true,
            int start = default(int),
            int count = default(int));

        /// <summary>
        /// 获取list
        /// </summary>
        /// <param name="where"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<T> GetList(Expression<Func<T, bool>> where, int count = default(int));
        Task<List<T>> GetListAsync(Expression<Func<T, bool>> where, int count = default(int));

        /// <summary>
        /// 查询第一个
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        T GetFirst(Expression<Func<T, bool>> where);
        Task<T> GetFirstAsync(Expression<Func<T, bool>> where);

        /// <summary>
        /// 查询记录数（判断记录是否存在请使用Exist方法，效率更高）
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        int GetCount(Expression<Func<T, bool>> where);
        Task<int> GetCountAsync(Expression<Func<T, bool>> where);

        /// <summary>
        /// 查询是否存在 
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        bool Exist(Expression<Func<T, bool>> where);
        Task<bool> ExistAsync(Expression<Func<T, bool>> where);
        #endregion

        /// <summary>
        /// 获取IQueryable对象，用于linq查询
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="Transaction"></param>
        void PrepareIQueryable(Func<IQueryable<T>, bool> callback, bool track = true);
        Task PrepareIQueryableAsync(Func<IQueryable<T>, Task<bool>> callback, bool track = true);

        void PrepareSession(Func<DbContext, bool> callback);
        Task PrepareSessionAsync(Func<DbContext, Task<bool>> callback);
    }
}
