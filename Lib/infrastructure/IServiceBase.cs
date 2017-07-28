using Lib.core;
using Lib.data;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Lib.infrastructure
{
    public interface IServiceBase<T> where T : class, IDBTable
    {
        string SUCCESS { get; }


        string CheckModel(T model);

        bool CheckModel(T model, out string msg);

        List<string> CheckEntity(T model);
    }
}
