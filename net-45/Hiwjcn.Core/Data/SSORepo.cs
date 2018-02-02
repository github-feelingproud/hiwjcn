using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using Lib.data.ef;

namespace Hiwjcn.Core.Data
{
    public interface ISSORepository<T> : IEFRepository<T>
        where T : IDBTable
    {
        //
    }

    public class SSORepository<T> : EFRepositoryFromContext<T, SSODB>,
        ISSORepository<T>
        where T : class, IDBTable
    {
        //
    }
}
