using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using Lib.mvc;

namespace Lib.infrastructure.service.user
{
    public abstract class RoleServiceBase
    {
        public async Task<_<string>> SetRolePermissions(string role_uid, string[] permissions)
        {
            var data = new _<string>();

            var old_permissions = new string[] { };

            var update = old_permissions.UpdateList(permissions);

            var dead_permissions = update.WaitForDelete;
            var new_permissions = update.WaitForAdd;

            //add new
            //delete old

            await Task.FromResult(1);

            return data;
        }
    }
}
