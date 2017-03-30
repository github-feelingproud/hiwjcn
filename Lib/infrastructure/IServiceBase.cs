using Lib.core;
using Lib.data;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Lib.infrastructure
{
    public interface IServiceBase<T> where T : class, IDBTable
    {
        bool UseCache { get; set; }

        int CacheExpiresMinutes { get; set; }

        void SetUseCacheValueTemporary(bool usecache);

        void RestoreUseCacheValue();

        string SUCCESS { get; }

        string CheckModel(T model);

        List<string> CheckEntity(T model);

        T FindFirstEntity(Expression<Func<T, bool>> where);

        string AddEntity(T model);

        string DeleteSingleEntity(Expression<Func<T, bool>> where);

        string DeleteSingleEntity(Expression<Func<T, bool>> where, Func<T, bool> CanDelete);

        string UpdateSingleEntity(Expression<Func<T, bool>> where, RefFunc<T, string> handler);
    }
}
