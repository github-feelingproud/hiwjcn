using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;

namespace Lib.infrastructure.entity.common
{
    [Serializable]
    public class EventLogEntityBase : BaseEntity
    {
        public virtual string Summary { get; set; }

        public virtual string Detail { get; set; }

        public virtual string JsonDescription { get; set; }

        public virtual string ExceptionInfo { get; set; }

        public virtual string UserUID { get; set; }

        public virtual int Priority { get; set; }

        public virtual string LogType { get; set; }
    }
}
