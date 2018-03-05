using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.rpc
{
    public enum ClientTypeEnum : int
    {
        WebApi = 1,
        Wcf = 2
    }

    public class FancyClient<T>
    {
        public FancyClient()
        {
            if (!typeof(T).IsInterface) { throw new Exception("T must be interface"); }


        }

        public T Instance
        {
            get => throw new NotImplementedException();
        }
    }
}
