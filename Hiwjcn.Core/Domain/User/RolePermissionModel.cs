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
    /// <summary>
    /// 角色权限关联
    /// </summary>
    [Table("wp_role_permission")]
    public class RolePermissionModel : BaseEntity
    {
        /// <summary>
        /// 关联ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("role_permission_id")]
        public virtual int RolePermissionID { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        [Column("role_id")]
        public virtual int RoleID { get; set; }

        /// <summary>
        /// 权限ID
        /// </summary>
        [Column("permission_id")]
        public virtual string PermissionID { get; set; }
    }
}
