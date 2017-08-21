using Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebLogic.Model.User
{
    [Table("account_permission")]
    public class PermissionModel : BaseEntity
    {
        [MaxLength(20)]
        [Required]
        public virtual string Name { get; set; }

        [MaxLength(500)]
        public virtual string Description { get; set; }
    }

    /// <summary>
    /// 角色权限关联
    /// </summary>
    [Table("account_role_permission")]
    public class RolePermissionModel : BaseEntity
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [MaxLength(100)]
        [Required]
        public virtual string RoleID { get; set; }

        /// <summary>
        /// 权限ID
        /// </summary>
        [MaxLength(100)]
        [Required]
        public virtual string PermissionID { get; set; }
    }
}
