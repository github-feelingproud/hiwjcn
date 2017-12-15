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
    public class SettingEntityBase : BaseEntity
    {
        [Required]
        public virtual string Key { get; set; }

        public virtual string Value { get; set; }

        public virtual string UserUID { get; set; }

        public virtual string OrgUID { get; set; }

        public virtual string GroupKey { get; set; }
    }
}
