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

namespace Lib.infrastructure.service
{
    public abstract class PermissionServiceBase<PermissionBase>
        where PermissionBase : PermissionEntityBase
    {
        private readonly IEFRepository<PermissionBase> _perRepo;

        public PermissionServiceBase(IEFRepository<PermissionBase> _perRepo)
        {
            this._perRepo = _perRepo;
        }

        public virtual async Task<_<string>> AddPermission(PermissionBase model) =>
            await this._perRepo.AddTreeNode(model, "per");

        public virtual async Task<_<string>> DeletePermissionWhenNoChildren(string permission_uid) =>
            await this._perRepo.DeleteSingleNodeWhenNoChildren(permission_uid);

        public abstract void UpdatePermissionEntity(ref PermissionBase old_per, ref PermissionBase new_per);

        public virtual async Task<_<string>> UpdatePermission(PermissionBase model)
        {
            var data = new _<string>();
            var per = await this._perRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(per, "权限不存在");
            this.UpdatePermissionEntity(ref per, ref model);
            per.Update();

            if (!per.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }

            if (await this._perRepo.UpdateAsync(per) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("更新权限错误");
        }
    }
}
