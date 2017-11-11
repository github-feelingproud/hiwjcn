using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.infrastructure.entity
{
    public class MenuEntityBase : TreeEntityBase
    {
        public virtual string MenuName { get; set; }

        public virtual string Description { get; set; }

        public virtual string Url { get; set; }

        public virtual int Sort { get; set; }

        public virtual string PermissionJson { get; set; }
    }
}
