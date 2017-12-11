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
    public interface IRoleService :
        IRoleServiceBase<RoleEntity, UserRoleEntity, RolePermissionEntity>
    { }

    public class RoleService :
        RoleServiceBase<RoleEntity, UserRoleEntity, RolePermissionEntity>,
        IRoleService
    {
        public RoleService(
            IEFRepository<RoleEntity> _roleRepo,
            IEFRepository<UserRoleEntity> _userRoleRepo,
            IEFRepository<RolePermissionEntity> _rolePermissionRepo) :
            base(_roleRepo, _userRoleRepo, _rolePermissionRepo)
        {

        }

        public override void UpdateRoleEntity(ref RoleEntity old_role, ref RoleEntity new_role)
        {
            old_role.RoleName = new_role.RoleName;
            old_role.RoleDescription = new_role.RoleDescription;
            old_role.AutoAssignRole = new_role.AutoAssignRole;
        }
    }
}
