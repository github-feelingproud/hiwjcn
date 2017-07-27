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
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(nameof(IID))]
        public virtual int IID { get; set; }

        [Index]
        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "客户端UID必填")]
        public virtual string UID { get; set; }

        public virtual string Name { get; set; }

        public virtual string Description { get; set; }

        public virtual DateTime CreateTime { get; set; }

        public virtual DateTime? UpdateTime { get; set; }
    }

    /// <summary>
    /// 角色权限关联
    /// </summary>
    [Table("account_role_permission")]
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
