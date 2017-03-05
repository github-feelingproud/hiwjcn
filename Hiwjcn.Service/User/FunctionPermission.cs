using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lib.core;
using Lib.helper;

namespace WebLogic.Bll.User
{
    /// <summary>
    /// 可用功能的枚举
    /// </summary>
    public enum FunctionsEnum : int
    {
        开店 = 1 << 0,
        购物 = 1 << 1,
        发帖 = 1 << 2,
        终极管理员 = 1 << 3,
        协助管理员 = 1 << 4,
        普通用户 = 1 << 5
    }

    /// <summary>
    /// 对用户使用某些功能进行权限验证
    /// =文档在邮箱里，搜索“关于PHP位运算的简单权限设计”=
    /// </summary>
    public static class FunctionPermission
    {
        /// <summary>
        /// 添加一个权限
        /// </summary>
        /// <param name="UserPermission"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        public static int AddPermission(int UserPermission, FunctionsEnum permission)
        {
            UserPermission = UserPermission | (int)permission;
            return UserPermission;
        }

        /// <summary>
        /// 删除一个权限
        /// </summary>
        /// <param name="UserPermission"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        public static int RemovePermission(int UserPermission, FunctionsEnum permission)
        {
            UserPermission &= ~(int)permission;
            return UserPermission;
        }

        /// <summary>
        /// 判断权限是否足够。
        /// 第一个参数是用户拥有的权限。
        /// 第二个是需要验证是否拥有的权限。
        /// 第二个参数可以多个权限分开写（per1,per2,per3）。
        /// 也可以一起写（per1|per2|per3）
        /// </summary>
        /// <param name="UserPermission"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public static bool AllPass(int UserPermission, params FunctionsEnum[] permissions)
        {
            if (!ValidateHelper.IsPlumpList(permissions)) { throw new Exception("权限参数为空"); }
            return permissions.All(x => HasPermission(UserPermission, x));
        }

        /// <summary>
        /// 只要拥有一个权限就通过验证
        /// </summary>
        /// <param name="UserPermission"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public static bool AnyPass(int UserPermission, params FunctionsEnum[] permissions)
        {
            if (!ValidateHelper.IsPlumpList(permissions)) { throw new Exception("权限参数为空"); }
            return permissions.Any(x => HasPermission(UserPermission, x));
        }

        /// <summary>
        /// 判断是否又权限
        /// </summary>
        /// <param name="user_permission"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        public static bool HasPermission(int user_permission, FunctionsEnum permission)
        {
            var per = (int)permission;
            return (user_permission & per) == per;
        }

    }
}
