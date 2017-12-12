using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Lib.helper;
using Lib.data;
using Lib.core;
using Lib.extension;
using Lib.infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Lib.infrastructure.entity
{
    /// <summary>
    /// 组织，公司
    /// </summary>
    [Serializable]
    public class OrganizationEntityBase : AreaEntityBase
    {
        [Required]
        public virtual string OrgName { get; set; }

        public virtual string OrgDescription { get; set; }

        public virtual string OrgImage { get; set; }

        public virtual string OrgWebSite { get; set; }

        public virtual string Phone { get; set; }

        public virtual DateTime? StartTime { get; set; }

        public virtual DateTime? EndTime { get; set; }

        [Required]
        public virtual string OwnerUID { get; set; }

        public virtual int MemeberCount { get; set; }
    }

    [Serializable]
    public class OrganizationMemberEntityBase : BaseEntity
    {
        [Required]
        public virtual string OrgUID { get; set; }

        [Required]
        public virtual string UserUID { get; set; }

        public virtual int Flag { get; set; }
    }

}
