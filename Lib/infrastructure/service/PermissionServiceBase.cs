using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;
using Lib.mvc;
using Lib.infrastructure.extension;
using Lib.data.ef;

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
    }
}
