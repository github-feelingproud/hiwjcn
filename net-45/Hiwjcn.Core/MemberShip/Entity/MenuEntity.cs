using Hiwjcn.Core.Data;
using Lib.infrastructure.entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiwjcn.Core.Domain
{
    [Serializable]
    [Table("tb_menu")]
    public class MenuEntity : MenuEntityBase, IMemberShipDBTable
    {
        [NotMapped]
        public virtual List<MenuEntity> Children { get; set; }

        [NotMapped]
        public virtual List<string> PermissionIds { get; set; }
    }
}
