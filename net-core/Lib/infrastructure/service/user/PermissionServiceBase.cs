using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;
using Lib.mvc;
using Lib.infrastructure.extension;
using Lib.data.ef;
using Lib.helper;
using Lib.extension;
using Lib.infrastructure.entity.user;

namespace Lib.infrastructure.service.user
{
    public interface IPermissionServiceBase<PermissionBase>
    {
        Task<_<PermissionBase>> AddPermission(PermissionBase model);

        Task<PermissionBase> GetPermissionByUID(string uid);

        Task<_<int>> DeletePermissionWhenNoChildren(string permission_uid);

        Task<List<PermissionBase>> QueryPermissionList(string parent = null);

        Task<_<PermissionBase>> UpdatePermission(PermissionBase model);
    }

    public abstract class PermissionServiceBase<PermissionBase> :
        IPermissionServiceBase<PermissionBase>
        where PermissionBase : PermissionEntityBase
    {
        protected readonly IEFRepository<PermissionBase> _permissionRepo;

        public PermissionServiceBase(IEFRepository<PermissionBase> _permissionRepo)
        {
            this._permissionRepo = _permissionRepo;
        }

        public virtual async Task<_<PermissionBase>> AddPermission(PermissionBase model) =>
            await this._permissionRepo.AddTreeNode(model, "per");

        public virtual async Task<_<int>> DeletePermissionWhenNoChildren(string permission_uid) =>
            await this._permissionRepo.DeleteSingleNodeWhenNoChildren_(permission_uid);

        public virtual async Task<List<PermissionBase>> QueryPermissionList(string parent = null) =>
            await this._permissionRepo.QueryNodeList(parent);

        public abstract void UpdatePermissionEntity(ref PermissionBase old_permission, ref PermissionBase new_permission);

        public virtual async Task<_<PermissionBase>> UpdatePermission(PermissionBase model)
        {
            var data = new _<PermissionBase>();
            var permission = await this._permissionRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(permission, $"权限为空:{model.UID}");
            this.UpdatePermissionEntity(ref permission, ref model);
            permission.Update();
            if (!permission.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await this._permissionRepo.UpdateAsync(permission) > 0)
            {
                data.SetSuccessData(permission);
                return data;
            }

            throw new Exception("更新权限错误");
        }

        public virtual async Task<PermissionBase> GetPermissionByUID(string uid) =>
            await this._permissionRepo.GetFirstAsync(x => x.UID == uid);
    }
}
