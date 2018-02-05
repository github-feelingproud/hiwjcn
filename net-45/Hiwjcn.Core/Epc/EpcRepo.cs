#define use_mysql

using Lib.data;
using Lib.data.ef;

namespace EPC.Core
{
    public interface IEpcRepository<T> :
        IEFRepository<T>
        where T : class, IDBTable
    {
        //
    }

    public class EpcRepository<T> :
        EFRepositoryFromContext<T, EpcEntityDB>,
        IEpcRepository<T>
        where T : class, IDBTable
    {
        //
    }
}
