using Model.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebLogic.Model.User
{
    public class LoginUserInfo
    {
        public LoginUserInfo() { }

        public LoginUserInfo(UserModel model)
        {
            this.UserID = model.UserID;
            this.NickName = model.NickName;
        }

        public virtual int UserID { get; set; }

        public virtual string NickName { get; set; }

        public virtual string Email { get; set; }

        public virtual string LoginToken { get; set; }

        public virtual IList<string> Permissions { get; set; }

        /// <summary>
        /// 判断用户是否有权限
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public bool HasPermission(string permission)
        {
            return this.Permissions != null && this.Permissions.Contains(permission);
        }
    }
}
