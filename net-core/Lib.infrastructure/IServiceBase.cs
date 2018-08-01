using Lib.data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lib.infrastructure
{
    public interface IServiceBase<T> where T : class, IDBTable
    {
        string SUCCESS { get; }

        string CheckModel(T model);

        bool CheckModel(T model, out string msg);

        List<string> CheckEntity(T model);

        void CustomCheckModel(ref T model, ref List<string> errors);
    }

    /// <summary>
    /// 保存的时候检查，是否有重复之类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISaveCheck<T> where T : class, IDBTable
    {
        Task<_<string>> CheckEntityWhenAdd(T model);

        Task<_<string>> CheckEntityWhenUpdate(T model);
    }
}
