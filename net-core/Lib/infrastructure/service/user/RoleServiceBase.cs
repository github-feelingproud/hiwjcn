using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;
using Lib.data;
using Lib.mvc;
using Lib.helper;
using Lib.extension;
using System.Data.Entity;
using Lib.core;
using Lib.mvc.user;
using Lib.cache;
using Lib.infrastructure.extension;
using Lib.infrastructure.entity.user;
using Lib.data.ef;

namespace Lib.infrastructure.service.user
{
    public interface IRoleServiceBase<RoleBase, UserRoleBase, RolePermissionBase>
    {
        Task<List<RoleBase>> QueryRoleList(string parent = null);

        Task<_<RoleBase>> AddRole(RoleBase role);
        
        Task<_<int>> DeleteRoleWhenNoChildren(string uid);

        Task<_<RoleBase>> UpdateRole(RoleBase model);

        Task<_<string>> SetUserRoles(string user_uid, List<UserRoleBase> roles);

        Task<_<string>> SetRolePermissions(string role_uid, List<RolePermissionBase> permissions);
    }

    public abstract class RoleServiceBase<RoleBase, UserRoleBase, RolePermissionBase> :
        IRoleServiceBase<RoleBase, UserRoleBase, RolePermissionBase>
        where RoleBase : RoleEntityBase, new()
        where RolePermissionBase : RolePermissionEntityBase, new()
        where UserRoleBase : UserRoleEntityBase, new()
    {
        protected readonly IEFRepository<RoleBase> _roleRepo;
        protected readonly IEFRepository<UserRoleBase> _userRoleRepo;
        protected readonly IEFRepository<RolePermissionBase> _rolePermissionRepo;

        public RoleServiceBase(
            IEFRepository<RoleBase> _roleRepo,
            IEFRepository<UserRoleBase> _userRoleRepo,
            IEFRepository<RolePermissionBase> _rolePermissionRepo)
        {
            this._roleRepo = _roleRepo;
            this._userRoleRepo = _userRoleRepo;
            this._rolePermissionRepo = _rolePermissionRepo;
        }

        public virtual async Task<List<RoleBase>> QueryRoleList(string parent = null) =>
            await this._roleRepo.QueryNodeList(parent);

        public virtual async Task<_<RoleBase>> AddRole(RoleBase role) =>
            await this._roleRepo.AddTreeNode(role, "role");
        
        public abstract void UpdateRoleEntity(ref RoleBase old_role, ref RoleBase new_role);

        public virtual async Task<_<RoleBase>> UpdateRole(RoleBase model)
        {
            var data = new _<RoleBase>();

            var role = await this._roleRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(role, $"角色不存在：{model.UID}");
            this.UpdateRoleEntity(ref role, ref model);
            role.Update();
            if (!role.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await this._roleRepo.UpdateAsync(role) > 0)
            {
                data.SetSuccessData(role);
                return data;
            }

            throw new Exception("更新角色失败");
        }

        public virtual async Task<_<string>> SetUserRoles(string user_uid, List<UserRoleBase> roles)
        {
            var data = new _<string>();
            if (ValidateHelper.IsPlumpList(roles))
            {
                if (roles.Any(x => x.UserID != user_uid))
                {
                    data.SetErrorMsg("用户ID错误");
                    return data;
                }
            }
            await this._userRoleRepo.DeleteWhereAsync(x => x.UserID == user_uid);
            if (ValidateHelper.IsPlumpList(roles))
            {
                foreach (var m in roles)
                {
                    m.Init("ur");
                    if (!m.IsValid(out var msg))
                    {
                        data.SetErrorMsg(msg);
                        return data;
                    }
                }
                if (await this._userRoleRepo.AddAsync(roles.ToArray()) <= 0)
                {
                    data.SetErrorMsg("保存角色错误");
                    return data;
                }
            }

            data.SetSuccessData(string.Empty);
            return data;
        }

        public virtual async Task<_<string>> SetRolePermissions(string role_uid, List<RolePermissionBase> permissions)
        {
            var data = new _<string>();
            //检查参数
            if (permissions.Any(x => x.RoleID != role_uid))
            {
                data.SetErrorMsg("角色ID错误");
                return data;
            }

            //旧的权限
            var old_per = await this._rolePermissionRepo.GetListAsync(x => x.RoleID == role_uid);

            //要更新的数据
            var update = old_per.UpdateList(permissions, x => x.UID);

            //等待添加
            if (ValidateHelper.IsPlumpList(update.WaitForAdd))
            {
                var add_list = permissions.Where(x => update.WaitForAdd.Contains(x.UID)).ToList();
                foreach (var m in add_list)
                {
                    m.Init("per");
                    if (!m.IsValid(out var msg))
                    {
                        data.SetErrorMsg(msg);
                        return data;
                    }
                }

                await this._rolePermissionRepo.AddAsync(add_list.ToArray());
            }
            //等待删除
            if (ValidateHelper.IsPlumpList(update.WaitForDelete))
            {
                var delete_list = update.WaitForDelete.ToList();

                await this._rolePermissionRepo.DeleteWhereAsync(x => delete_list.Contains(x.UID));
            }

            data.SetSuccessData(string.Empty);
            return data;
        }

        public async Task<_<int>> DeleteRoleWhenNoChildren(string uid) =>
            await this._roleRepo.DeleteSingleNodeWhenNoChildren_(uid);
    }
}
