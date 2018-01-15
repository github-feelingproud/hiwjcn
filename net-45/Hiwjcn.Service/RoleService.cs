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
        Task<RoleEntity> GetRoleByUID(string uid);
    }

    public class RoleService : RoleServiceBase<RoleEntity, UserRoleEntity, RolePermissionEntity>,
        IRoleService
    {
        public RoleService(
            IEFRepository<RoleEntity> _roleRepo,
            IEFRepository<UserRoleEntity> _userRoleRepo,
            IEFRepository<RolePermissionEntity> _rolePermissionRepo) :
            base(_roleRepo, _userRoleRepo, _rolePermissionRepo)
        { }

        public async Task<RoleEntity> GetRoleByUID(string uid)
        {
            var role = await this._roleRepo.GetFirstAsync(x => x.UID == uid);
            if (role != null)
            {
                var map = await this._rolePermissionRepo.GetListAsync(x => x.RoleID == uid);
                role.PermissionIds = map.Select(x => x.PermissionID).ToList();

                role.Children = await this._roleRepo.GetListAsync(x => x.ParentUID == role.UID);
            }
            return role;
        }

        public override void UpdateRoleEntity(ref RoleEntity old_role, ref RoleEntity new_role)
        {
            old_role.RoleName = new_role.RoleName;
            old_role.RoleDescription = new_role.RoleDescription;
            old_role.AutoAssignRole = new_role.AutoAssignRole;
        }

        private async Task<bool> SaveRolePermissions(RoleEntity role, List<string> permissions)
        {
            if (!ValidateHelper.IsPlumpList(permissions)) { return true; }

            var data = permissions.Select(x => new RolePermissionEntity()
            {
                RoleID = role.UID,
                PermissionID = x,
            }.InitSelf("rp")).ToList();
            await this._rolePermissionRepo.AddAsync(data.ToArray());
            return true;
        }

        public override async Task<_<string>> UpdateRole(RoleEntity model)
        {
            var res = new _<string>();

            var entity = await this._roleRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(entity, "角色不存在");
            this.UpdateRoleEntity(ref entity, ref model);
            entity.Update();

            if (!entity.IsValid(out var msg))
            {
                res.SetErrorMsg(msg);
                return res;
            }

            await this._roleRepo.UpdateAsync(entity);

            await this._rolePermissionRepo.DeleteWhereAsync(x => x.RoleID == model.UID);

            await this.SaveRolePermissions(model, model.PermissionIds);

            res.SetSuccessData(string.Empty);
            return res;
        }

        public override async Task<_<string>> AddRole(RoleEntity model)
        {
            var res = new _<string>();

            model.Init("role");
            if (!model.IsValid(out var msg))
            {
                res.SetErrorMsg(msg);
                return res;
            }

            await this._roleRepo.AddAsync(model);

            await this.SaveRolePermissions(model, model.PermissionIds);

            res.SetSuccessData(string.Empty);
            return res;
        }
    }
}
