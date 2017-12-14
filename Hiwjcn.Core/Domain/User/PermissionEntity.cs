using Lib.infrastructure.entity;
using Lib.infrastructure.entity.user;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hiwjcn.Core.Domain.User
{
    [Table("account_permission")]
    public class PermissionEntity : PermissionEntityBase { }
}
