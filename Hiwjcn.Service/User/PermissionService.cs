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
    public interface IPermissionService : IPermissionServiceBase<PermissionEntity>
    { }

    public class PermissionService : PermissionServiceBase<PermissionEntity>,
        IPermissionService
    {
        public PermissionService(IEFRepository<PermissionEntity> _perRepo) : base(_perRepo)
        { }

        public override void UpdatePermissionEntity(ref PermissionEntity old_permission, ref PermissionEntity new_permission)
        {
            old_permission.Name = new_permission.Name;
            old_permission.Description = new_permission.Description;
        }
    }
}
