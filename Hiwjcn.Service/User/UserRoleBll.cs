using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebLogic.Model.User;
using Bll;
using Lib.core;
using WebLogic.Dal.User;
using Lib.helper;
using Lib.infrastructure;

namespace WebLogic.Bll.User
{
    /// <summary>
    /// 用户角色关联
    /// </summary>
    public class UserRoleBll : ServiceBase<UserRoleModel>
    {
        private UserRoleDal _UserRoleDal { get; set; }
        public UserRoleBll()
        {
            this._UserRoleDal = new UserRoleDal();
        }

        public override string CheckModel(UserRoleModel model)
        {
            if (model == null) { return "用户角色关联对象为空"; }
            return string.Empty;
        }

        /// <summary>
        /// 添加用户角色关联
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AddUserRole(UserRoleModel model)
        {
            string errinfo = CheckModel(model);
            if (ValidateHelper.IsPlumpString(errinfo)) { return errinfo; }

            return _UserRoleDal.Add(model) > 0 ? SUCCESS : "添加用户角色关联失败";
        }

        /// <summary>
        /// 删除用户角色关联
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="role_id"></param>
        /// <returns></returns>
        public string DeleteUserRole(string user_id, string role_id)
        {
            var list = _UserRoleDal.GetList(x => x.UserID == user_id && x.RoleID == role_id);
            if (!ValidateHelper.IsPlumpList(list)) { return "您要删除的数据不存在"; }
            return _UserRoleDal.Delete(list.ToArray()) > 0 ? SUCCESS : "删除用户角色关联失败";
        }

        /// <summary>
        /// 获取用户的角色
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public List<string> GetRolesByUserID(string uid)
        {
            var list = _UserRoleDal.GetList(x => x.UserID == uid);
            if (ValidateHelper.IsPlumpList(list))
            {
                return list.Select(x => x.UID).Distinct().ToList();
            }
            return null;
        }

    }
}
