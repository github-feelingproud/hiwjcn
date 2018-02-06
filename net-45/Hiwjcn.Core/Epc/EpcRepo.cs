#define use_mysql

using Lib.data;
using Lib.data.ef;
using Lib.infrastructure.entity;

namespace EPC.Core
{
    public interface IEpcDBTable : IDBTable { }

    public interface IEpcRepository<T> :
        IEFRepository<T>
        where T : class, IEpcDBTable
    {
        //
    }

    public class EpcRepository<T> : EFRepositoryFromContext<T, EpcEntityDB>,
        IEpcRepository<T>
        where T : class, IEpcDBTable
    {
        //
    }
}
