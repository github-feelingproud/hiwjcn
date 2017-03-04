using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebLogic.Bll.User;
using WebLogic.Model.User;

namespace Hiwjcn.Web.Models.Permission
{
    public class RolePermissionViewModel : ViewModelBase
    {
        public virtual RoleModel Role { get; set; }

        public virtual List<PermissionRecord> AssignedList { get; set; }

        public virtual List<PermissionRecord> UnAssignedList { get; set; }
    }
}