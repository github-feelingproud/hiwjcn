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
using Lib.mvc;
using Lib.helper;
using Lib.infrastructure.extension;

namespace Hiwjcn.Bll.User
{
    public interface IRoleService : IRoleServiceBase<RoleEntity, UserRoleEntity, RolePermissionEntity>,
        IAutoRegistered
    {
        Task<List<RoleEntity>> LoadPermissionIds(List<RoleEntity> data);
    }

    public class RoleService : RoleServiceBase<RoleEntity, UserRoleEntity, RolePermissionEntity>,
        IRoleService
    {
        public RoleService(
            IEFRepository<RoleEntity> _roleRepo,
            IEFRepository<UserRoleEntity> _userRoleRepo,
            IEFRepository<RolePermissionEntity> _rolePermissionRepo) :
            base(_roleRepo, _userRoleRepo, _rolePermissionRepo)
        {
            //
        }
        
        public async Task<List<RoleEntity>> LoadPermissionIds(List<RoleEntity> data)
        {
            if (ValidateHelper.IsPlumpList(data))
            {
                var uids = data.Select(x => x.UID).ToList();
                var map = await this._rolePermissionRepo.GetListAsync(x => uids.Contains(x.RoleID));
                foreach (var role in data)
                {
                    role.PermissionIds = map.Where(x => x.RoleID == role.UID).Select(x => x.PermissionID).ToList();
                }
            }

            return data;
        }

        public override void UpdateRoleEntity(ref RoleEntity old_role, ref RoleEntity new_role)
        {
            old_role.RoleName = new_role.RoleName;
            old_role.RoleDescription = new_role.RoleDescription;
            old_role.AutoAssignRole = new_role.AutoAssignRole;
        }
    }
}
