using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using Lib.data.ef;

namespace Hiwjcn.Core.Data
{
    public interface IMemberShipDBTable : IDBTable { }

    public interface IMSRepository<T> : IEFRepository<T>
        where T : IMemberShipDBTable
    {
        //
    }

    public class MemberShipRepository<T> : EFRepository<T>,
        IMSRepository<T>
        where T : class, IMemberShipDBTable
    {
        //
    }
}
