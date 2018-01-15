using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hiwjcn.Core.Domain.User;
using Lib.infrastructure.service;
using Lib.infrastructure.entity;
using Lib.ioc;
using Lib.data.ef;
using Lib.mvc.user;
using Lib.extension;
using Lib.infrastructure.service.user;

namespace Hiwjcn.Bll.User
{
    public interface IDepartmentService :
        IDepartmentServiceBase<DepartmentEntity, UserDepartmentEntity, DepartmentRoleEntity>,
        IAutoRegistered
    {
        //
    }

    public class DepartmentService :
        DepartmentServiceBase<DepartmentEntity, UserDepartmentEntity, DepartmentRoleEntity>,
        IDepartmentService
    {
        public DepartmentService(
            IEFRepository<DepartmentEntity> _departmentRepo,
            IEFRepository<UserDepartmentEntity> _userDepartmentRepo,
            IEFRepository<DepartmentRoleEntity> _departmentRoleRepo) :
            base(_departmentRepo, _userDepartmentRepo, _departmentRoleRepo)
        { }

        public override void UpdateDepartmentEntity(ref DepartmentEntity old_department, ref DepartmentEntity new_department)
        {
            old_department.DepartmentName = new_department.DepartmentName;
            old_department.Description = new_department.Description;
        }
    }
}
