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
        int Add(params T[] models);
        Task<int> AddAsync(params T[] models);
        int Delete(params T[] models);
        int Update(params T[] models);
        T GetByKeys(params object[] keys);
        List<T> QueryList<OrderByColumnType>(
            Expression<Func<T, bool>> where,
            Expression<Func<T, OrderByColumnType>> orderby = null,
            bool Desc = true,
            int start = default(int),
            int count = default(int));
        List<T> GetList(Expression<Func<T, bool>> where, int count = default(int));
        T GetFirst(Expression<Func<T, bool>> where);
        int GetCount(Expression<Func<T, bool>> where);
        bool Exist(Expression<Func<T, bool>> where);
        void PrepareSession(Func<DbContext, bool> callback);
        //void PrepareConnection(Action<IDbConnection> callback);
        //void PrepareConnection(Func<IDbConnection, IDbTransaction, bool> callback, IsolationLevel? isoLevel = null);
        void PrepareIQueryable(Func<IQueryable<T>, bool> callback, bool track = true);
    }
}
