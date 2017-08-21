using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lib.extension;
using Lib.ioc;

namespace Lib.mvc.user
{
    /// <summary>
    /// 记录登陆信息，可以序列化
    /// </summary>
    [Serializable]
    public class LoginUserInfo
    {
        public LoginUserInfo() { }

        public virtual int IID { get; set; }

        public virtual string UserID { get; set; }

        [Obsolete("等同于UserID")]
        public virtual string UserUID
        {
            get { return this.UserID; }
            set { this.UserID = value; }
        }

        public virtual string UserName { get; set; }

        public virtual string NickName { get; set; }

        public virtual string Email { get; set; }

        public virtual string HeadImgUrl { get; set; }

        public virtual int IsActive { get; set; }

        public virtual bool IsValid { get; set; } = false;

        public virtual string LoginToken { get; set; }

        public virtual string RefreshToken { get; set; }

        public virtual DateTime TokenExpire { get; set; }

        public virtual List<string> Scopes { get; set; }

        public virtual List<string> Permissions { get; set; }

        public virtual Dictionary<string, string> ExtraData { get; set; }


        public static implicit operator LoginUserInfo(string json) => json.JsonToEntity<LoginUserInfo>();
    }

    public static class LoginUserInfoExtension
    {
        public static string UserNameOrNickName(this LoginUserInfo loginuser) => loginuser.NickName ?? loginuser.UserName;

        /// <summary>
        /// 去除权限等敏感信息
        /// </summary>
        public static void ClearPrivateInfo(this LoginUserInfo loginuser)
        {
            loginuser.Permissions?.Clear();
            loginuser.Scopes?.Clear();
        }

        /// <summary>
        /// 判断用户是否有权限
        /// </summary>
        public static bool HasPermission(this LoginUserInfo loginuser, string permission) =>
            ValidateHelper.IsPlumpList(loginuser.Permissions) && loginuser.Permissions.Contains(permission);


        /// <summary>
        /// 判断是否有scope
        /// </summary>
        public static bool HasScope(this LoginUserInfo loginuser, string scope) =>
            ValidateHelper.IsPlumpList(loginuser.Scopes) && loginuser.Scopes.Contains(scope);

        /// <summary>
        /// 添加额外信息
        /// </summary>
        public static void AddExtraData(this LoginUserInfo loginuser, string key, string value)
        {
            if (loginuser.ExtraData == null) { loginuser.ExtraData = new Dictionary<string, string>(); }
            loginuser.ExtraData[key] = value;
        }

        /// <summary>
        /// 获取额外信息
        /// </summary>
        public static string GetExtraData(this LoginUserInfo loginuser, string key)
        {
            if (loginuser.ExtraData == null) { loginuser.ExtraData = new Dictionary<string, string>(); }
            if (loginuser.ExtraData.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

    }
}
