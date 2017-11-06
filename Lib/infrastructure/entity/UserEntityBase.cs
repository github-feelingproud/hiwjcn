using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.infrastructure.entity
{
    public class UserEntityBase : BaseEntity
    { }

    [Serializable]
    public class UserAvatarEntityBase : BaseEntity
    {
        [Required(ErrorMessage = "头像数据为空")]
        public virtual byte[] AvatarBytes { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "UserUID长度错误")]
        [Required(ErrorMessage = "UserUID必填")]
        [Index]
        public virtual string UserUID { get; set; }
    }

    [Serializable]
    public class UserOneTimeCodeEntityBase : BaseEntity
    {
        [StringLength(100)]
        [Required]
        public virtual string Code { get; set; }

        [StringLength(100)]
        [Required]
        public virtual string UserUID { get; set; }

        public virtual int CodeType { get; set; }
    }

    [Serializable]
    public class RoleEntityBase : BaseEntity
    {
        /// <summary>
        /// 角色名
        /// </summary>
        [Column("role_name")]
        [MaxLength(20)]
        [Required]
        public virtual string RoleName { get; set; }

        /// <summary>
        /// 角色描述
        /// </summary>
        [Column("role_desc")]
        [MaxLength(500)]
        public virtual string RoleDescription { get; set; }

        /// <summary>
        /// 用户注册的时候自动分配这些权限
        /// </summary>
        [Column("auto_assign_to_user")]
        public virtual int AutoAssignRole { get; set; }
    }

    [Serializable]
    public class PermissionEntityBase : BaseEntity
    {
        [MaxLength(20)]
        [Required]
        public virtual string Name { get; set; }

        [MaxLength(500)]
        public virtual string Description { get; set; }
    }

    [Serializable]
    public class RolePermissionEntityBase : BaseEntity
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

    [Serializable]
    public class UserRoleEntityBase : BaseEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Column("user_id")]
        [StringLength(100)]
        public virtual string UserID { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        [Column("role_id")]
        [StringLength(100)]
        public virtual string RoleID { get; set; }
    }
}
