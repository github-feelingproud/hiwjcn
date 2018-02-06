using Hiwjcn.Core.Data;
using Lib.core;
using Lib.data.ef;
using Lib.helper;
using Lib.infrastructure.entity;
using Lib.infrastructure.entity.user;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hiwjcn.Core.Entity
{
    [Serializable]
    [Table("tb_system")]
    public class SystemEntity : BaseEntity, IMemberShipDBTable
    {
        [Required]
        public virtual string Name { get; set; }

        [Required]
        public virtual string Flag { get; set; }

        public virtual string Description { get; set; }

        public virtual string ImageUrl { get; set; }

        public virtual string Url { get; set; }
    }
}
