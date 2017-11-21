using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.distributed.zookeeper.ServiceManager
{
    public class AddressModel
    {
        public virtual string Url { get; set; }

        public virtual string Ip { get; set; }

        public virtual DateTime? OnLineTime { get; set; }
    }
}
