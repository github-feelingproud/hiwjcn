using Lib.infrastructure.entity;
using Lib.infrastructure.entity.user;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hiwjcn.Core.Domain.User
{
    [Serializable]
    [Table("tb_department")]
    public class DepartmentEntity : DepartmentEntityBase
    {
        //
    }

    [Serializable]
    [Table("tb_user_department")]
    public class UserDepartmentEntity : UserDepartmentEntityBase
    {
        //
    }

    [Serializable]
    [Table("tb_department_role")]
    public class DepartmentRoleEntity : DepartmentRoleEntityBase
    {
        //
    }

}
