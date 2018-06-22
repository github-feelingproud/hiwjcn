using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.infrastructure.model
{

    [Serializable]
    public class ScopeInfoModel
    {
        public virtual string uid { get; set; }
        public virtual string name { get; set; }
    }
}
