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
    /// 获取最新的用户权限
    /// </summary>
    public interface IFetchUserPermissions
    {
        /// <summary>
        /// 获取最新的用户权限
        /// </summary>
        /// <param name="loginuser"></param>
        /// <returns></returns>
        List<string> FetchPermission(LoginUserInfo loginuser);
    }

    /// <summary>
    /// 记录登陆信息，可以序列化
    /// </summary>
    [Serializable]
    public class LoginUserInfo
    {
        public LoginUserInfo() { }

        [Obsolete("计划废止，推荐使用UserID")]
        public virtual int IID { get; set; }

        public virtual string UserID { get; set; }

        [Obsolete("计划废止，推荐使用UserID")]
        public virtual string UserUID
        {
            get { return this.UserID; }
            set { this.UserID = value; }
        }

        public virtual string NickName { get; set; }

        [Obsolete("不作为ID使用，推荐使用UserID")]
        public virtual string Email { get; set; }

        public virtual string HeadImgUrl { get; set; }

        public virtual int IsActive { get; set; }

        public virtual bool IsValid { get; set; } = false;

        public virtual string LoginToken { get; set; }

        public virtual string RefreshToken { get; set; }

        public virtual DateTime TokenExpire { get; set; }

        public virtual List<string> Permissions { get; set; }

        /// <summary>
        /// 判断用户是否有权限
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public virtual bool HasPermission(string permission)
        {
            return ValidateHelper.IsPlumpList(this.Permissions) && this.Permissions.Contains(permission);
        }

        public virtual List<string> Scopes { get; set; }

        /// <summary>
        /// 判断是否有scope
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public virtual bool HasScope(string scope) => ValidateHelper.IsPlumpList(this.Scopes) && this.Scopes.Contains(scope);

        public virtual Dictionary<string, string> ExtraData { get; set; }

        public virtual void AddExtraData(string key, string value)
        {
            if (this.ExtraData == null) { this.ExtraData = new Dictionary<string, string>(); }
            this.ExtraData[key] = value;
        }

        public virtual string GetExtraData(string key)
        {
            if (this.ExtraData == null) { this.ExtraData = new Dictionary<string, string>(); }
            if (this.ExtraData.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

        /// <summary>
        /// //////////////////////////////////////////////////////////////////////////
        /// </summary>
        [Obsolete("请使用ExtraData存放")]
        public virtual string TraderName { get; set; }
        [Obsolete("请使用ExtraData存放")]
        public virtual int IsCheck { get; set; }
        [Obsolete("请使用ExtraData存放")]
        public virtual string CustomerType { get; set; }
        [Obsolete("请使用ExtraData存放")]
        public virtual int IsHaveInquiry { get; set; }
        [Obsolete("请使用ExtraData存放")]
        public virtual int IsSelf { get; set; }

        public static implicit operator LoginUserInfo(string json) => json.JsonToEntity<LoginUserInfo>();
    }
}
