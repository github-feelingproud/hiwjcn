using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebLogic.Model.User;
using Bll;
using Lib.core;
using WebLogic.Model.User;
using WebLogic.Dal.User;
using Lib.helper;

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
            if (model.RoleID <= 0) { return "角色ID为空"; }
            if (model.UserID <= 0) { return "用户ID为空"; }
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
        public string DeleteUserRole(int user_id, int role_id)
        {
            if (user_id <= 0 || role_id <= 0) { return "参数错误"; }

            var list = _UserRoleDal.GetList(x => x.UserID == user_id && x.RoleID == role_id);
            if (!ValidateHelper.IsPlumpList(list)) { return "您要删除的数据不存在"; }
            return _UserRoleDal.Delete(list.ToArray()) > 0 ? SUCCESS : "删除用户角色关联失败";
        }

        /// <summary>
        /// 获取用户的角色
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public List<int> GetRolesByUserID(int uid)
        {
            if (uid <= 0) { return null; }
            string key = Com.GetCacheKey("GetRolesByUserID:", uid.ToString());
            return Cache(key, () =>
            {
                var list = _UserRoleDal.GetList(x => x.UserID == uid);
                if (ValidateHelper.IsPlumpList(list))
                {
                    return list.Select(x => x.RoleID).Distinct().ToList();
                }
                return null;
            });
        }

    }
}
