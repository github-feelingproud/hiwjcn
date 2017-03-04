using Model.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebLogic.Model.User;

namespace Hiwjcn.Web.Models.Permission
{
    public class UserRoleViewModel : ViewModelBase
    {
        public virtual UserModel User { get; set; }

        public virtual List<RoleModel> AssignedList { get; set; }

        public virtual List<RoleModel> UnAssignedList { get; set; }
    }
}