using Lib.infrastructure.entity;
using Lib.infrastructure.entity.user;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hiwjcn.Core.Domain.User
{
    [Serializable]
    [Table("account_department")]
    public class DepartmentEntity : DepartmentEntityBase { }

    [Serializable]
    [Table("account_department_role")]
    public class DepartmentRoleEntity : DepartmentRoleEntityBase { }

    [Serializable]
    [Table("account_user_department")]
    public class UserDepartmentEntity : UserDepartmentEntityBase { }

}
