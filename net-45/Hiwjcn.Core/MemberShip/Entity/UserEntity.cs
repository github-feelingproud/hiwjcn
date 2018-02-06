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

namespace Hiwjcn.Core.Domain.User
{
    [Serializable]
    [Table("tb_user_avatar")]
    public class UserAvatarEntity : UserAvatarEntityBase, IMemberShipDBTable { }

    /// <summary>
    /// 一次性登录用的code
    /// </summary>
    [Serializable]
    [Table("tb_one_time_code")]
    public class UserOneTimeCodeEntity : UserOneTimeCodeEntityBase, IMemberShipDBTable { }

    [Serializable]
    [Table("tb_permission")]
    public class PermissionEntity : PermissionEntityBase, IMemberShipDBTable
    {
        [NotMapped]
        public virtual List<PermissionEntity> Children { get; set; }
    }

    /// <summary>
    /// 角色
    /// </summary>
    [Serializable]
    [Table("tb_role")]
    public class RoleEntity : RoleEntityBase, IMemberShipDBTable
    {
        [NotMapped]
        public virtual List<string> PermissionIds { get; set; }

        [NotMapped]
        public virtual List<RoleEntity> Children { get; set; }
    }

    /// <summary>
    /// 角色权限关联
    /// </summary>
    [Serializable]
    [Table("tb_role_permission")]
    public class RolePermissionEntity : RolePermissionEntityBase, IMemberShipDBTable { }

    /// <summary>
    /// 用户角色关联
    /// </summary>
    [Serializable]
    [Table("tb_user_role")]
    public class UserRoleEntity : UserRoleEntityBase, IMemberShipDBTable { }

    /// <summary>
    ///用户的账户模型
    /// </summary>
    [Serializable]
    [Table("tb_user")]
    public class UserEntity : UserEntityBase, IMemberShipDBTable
    {
        public override string Phone { get => base.Phone; set => base.Phone = value; }

        public new string Email { get => base.Email; set => base.Email = value; }

        /// <summary>
        /// 性别
        /// </summary>
        [Column("user_sex")]
        public virtual int Sex { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        [NotMapped]
        public virtual string UserToken { get; set; }

        [NotMapped]
        public virtual string SexName
        {
            get
            {
                try
                {
                    return ((SexEnum)this.Sex).ToString();
                }
                catch
                {
                    return this.Sex.ToString();
                }
            }
        }

        [NotMapped]
        public virtual string OrgUID { get; set; }

        [NotMapped]
        public virtual int OrgFlag { get; set; }

        [NotMapped]
        public virtual string OrgFlagName { get; set; }

        [NotMapped]
        public virtual string RoleNames { get; set; }
    }

    public class UserModelMapping : EFMappingBase<UserEntity>
    {
        public UserModelMapping()
        {
            this.ToTable("wp_users").HasKey(x => x.IID);
            this.Property(x => x.IID).HasColumnName("user_id").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.NickName).HasColumnName("nick_name");
            this.Ignore(x => x.RoleIds);
        }
    }
}