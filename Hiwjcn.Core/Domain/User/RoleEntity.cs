using Lib.infrastructure.entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hiwjcn.Core.Domain.User
{
    /// <summary>
    /// 角色
    /// </summary>
    [Table("account_role")]
    public class RoleEntity : RoleEntityBase { }

    /// <summary>
    /// 角色权限关联
    /// </summary>
    [Table("account_role_permission")]
    public class RolePermissionEntity : RolePermissionEntityBase { }

    /// <summary>
    /// 用户角色关联
    /// </summary>
    [Table("account_user_role")]
    public class UserRoleEntity : UserRoleEntityBase { }
}
