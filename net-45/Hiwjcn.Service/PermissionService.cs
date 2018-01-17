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

namespace Hiwjcn.Bll.User
{
    public interface IPermissionService : IPermissionServiceBase<PermissionEntity>,
        IAutoRegistered
    { }

    public class PermissionService : PermissionServiceBase<PermissionEntity>,
        IPermissionService
    {
        public PermissionService(IEFRepository<PermissionEntity> _perRepo) : base(_perRepo)
        { }

        public override void UpdatePermissionEntity(ref PermissionEntity old_permission, ref PermissionEntity new_permission)
        {
            old_permission.Description = new_permission.Description;
        }

        public override async Task<PermissionEntity> GetPermissionByUID(string uid)
        {
            var data = await base.GetPermissionByUID(uid);
            if (data != null)
            {
                data.Children = await this._permissionRepo.GetListAsync(x => x.ParentUID == data.UID);
            }
            return data;
        }

        public override async Task<_<PermissionEntity>> AddPermission(PermissionEntity model)
        {
            var res = new _<PermissionEntity>();
            if (await this._permissionRepo.ExistAsync(x => x.Name == model.Name))
            {
                res.SetErrorMsg("权限名已存在");
                return res;
            }

            return await base.AddPermission(model);
        }
    }
}
