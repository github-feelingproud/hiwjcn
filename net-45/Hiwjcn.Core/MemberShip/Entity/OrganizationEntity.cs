using Lib.infrastructure.entity;
using Lib.infrastructure.entity.auth;
using Lib.infrastructure.entity.user;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Hiwjcn.Core.Domain.User;
using System.ComponentModel;
using System.Collections.Generic;
using Lib.helper;
using System.Linq;
using Lib.extension;
using Hiwjcn.Core.Data;

namespace EPC.Core.Entity
{
    [Serializable]
    public enum MemberRoleEnum : int
    {
        管理员 = 1 << 0,

        普通成员 = 1 << 3,

        [Description("观察者-只能看")]
        观察者 = 1 << 4
    }

    public static class MemberRoleHelper
    {
        public static bool IsValid(int flag) =>
            PermissionHelper.HasAnyPermission(flag, GetRoles().Select(x => x.Value).ToArray());

        public static Dictionary<string, int> GetRoles() =>
            typeof(MemberRoleEnum).GetEnumFieldsValues().ToDictionary(x => x.Key, x => (int)x.Value);

        public static List<string> ParseRoleNames(int flag, Dictionary<string, int> all_roles = null)
        {
            all_roles = all_roles ?? GetRoles();

            return all_roles.Where(x => PermissionHelper.HasPermission(flag, x.Value)).Select(x => x.Key).ToList();
        }

    }

    [Serializable]
    [Table("tb_org")]
    public class OrganizationEntity : BaseEntity, IMemberShipDBTable
    {
        [Required]
        [Index(IsUnique = true)]
        public virtual string OrgName { get; set; }

        public virtual string OrgDescription { get; set; }

        public virtual string OrgImage { get; set; }

        public virtual string OrgWebSite { get; set; }

        public virtual string Phone { get; set; }

        public virtual DateTime? StartTime { get; set; }

        public virtual DateTime? EndTime { get; set; }

        [Required]
        public virtual string OwnerUID { get; set; }

        [Required]
        public virtual string UserUID { get; set; }

        public virtual int MemeberCount { get; set; }

        [NotMapped]
        public virtual UserEntity OwnerModel { get; set; }

        [NotMapped]
        public virtual string OwnerNickName { get => this.OwnerModel?.NickName; }

        [NotMapped]
        public virtual string OwnerEmail { get => this.OwnerModel?.Email; }
    }

    [Serializable]
    [Table("tb_org_memeber")]
    public class OrganizationMemberEntity : BaseEntity, IMemberShipDBTable
    {
        [Required]
        public virtual string OrgUID { get; set; }

        [Required]
        public virtual string UserUID { get; set; }

        /// <summary>
        /// 权限/角色
        /// </summary>
        public virtual int Flag { get; set; }

        public virtual int IsOwner { get; set; }

        /// <summary>
        /// 会员同意
        /// </summary>
        public virtual int MemberApproved { get; set; }

        /// <summary>
        /// 组织同意
        /// </summary>
        public virtual int OrgApproved { get; set; }
    }

}
