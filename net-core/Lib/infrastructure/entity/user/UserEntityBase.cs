using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lib.infrastructure.entity.user
{
    [Serializable]
    public class UserEntityBase : BaseEntity
    {
        [Column("user_name")]
        [Index(IsUnique = true)]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "用户名长度不匹配")]
        public virtual string UserName { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [Column("nick_name")]
        [StringLength(10, MinimumLength = 1, ErrorMessage = "昵称长度不匹配")]
        public virtual string NickName { get; set; }

        /// <summary>
        /// md5加密的密码
        /// </summary>
        [Column("user_pass")]
        [StringLength(300, MinimumLength = 1, ErrorMessage = "密码长度错误")]
        public virtual string PassWord { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        [Column("user_phone")]
        [StringLength(50)]
        public virtual string Phone { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [Column("user_email")]
        [StringLength(50)]
        public virtual string Email { get; set; }

        /// <summary>
        /// 头像链接
        /// </summary>
        [Column("user_img")]
        [StringLength(1000)]
        public virtual string UserImg { get; set; }

        /// <summary>
        /// 用户拥有的权限（大分类）
        /// </summary>
        [Column("user_flag")]
        public virtual int Flag { get; set; }

        [Column("is_active")]
        public virtual int IsActive { get; set; }

        /// <summary>
        /// 部门id
        /// </summary>
        [NotMapped]
        public virtual List<string> DepartmentIds { get; set; }

        /// <summary>
        /// 用户所有角色id
        /// </summary>
        [NotMapped]
        public virtual List<string> RoleIds { get; set; }

        /// <summary>
        /// 用户所有权限id
        /// </summary>
        [NotMapped]
        public virtual List<string> PermissionIds { get; set; }

        /// <summary>
        /// 权限字符串（最终拿来验证用户行为是否有权限）
        /// </summary>
        [NotMapped]
        public virtual List<string> PermissionNames { get; set; }
    }

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
    public class RoleEntityBase : TreeEntityBase
    {
        /// <summary>
        /// 角色名
        /// </summary>
        [Column("role_name")]
        [StringLength(200)]
        [Required]
        public virtual string RoleName { get; set; }

        /// <summary>
        /// 角色描述
        /// </summary>
        [Column("role_desc")]
        [StringLength(500)]
        public virtual string RoleDescription { get; set; }
    }

    [Serializable]
    public class PermissionEntityBase : TreeEntityBase
    {
        [StringLength(200)]
        [Required]
        [Index(IsUnique = true)]
        public virtual string Name { get; set; }

        [StringLength(500)]
        public virtual string Description { get; set; }
    }

    [Serializable]
    public class RolePermissionEntityBase : BaseEntity
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        [StringLength(100)]
        [Required]
        public virtual string RoleID { get; set; }

        /// <summary>
        /// 权限ID
        /// </summary>
        [StringLength(100)]
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
        [StringLength(100), Required]
        public virtual string UserID { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        [Column("role_id")]
        [StringLength(100), Required]
        public virtual string RoleID { get; set; }
    }
}
