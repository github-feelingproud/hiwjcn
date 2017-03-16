using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lib.extension;

namespace Lib.mvc.user
{
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

        public virtual string LoginToken { get; set; }

        public virtual List<string> Permissions { get; set; }

        public virtual Dictionary<string, string> ExtraData { get; set; }

        public virtual T GetExtraData<T>(string key)
        {
            try
            {
                if (this.ExtraData != null && this.ExtraData.ContainsKey(key))
                {
                    return (T)Convert.ChangeType(this.ExtraData[key], typeof(T));
                }
                throw new Exception($"登录信息的额外数据中不存在key为{key}的数据");
            }
            catch (Exception e)
            {
                e.SaveLog("获取登录额外信息错误");
                return default(T);
            }
        }

        /// <summary>
        /// 判断用户是否有权限
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        public bool HasPermission(string permission)
        {
            return this.Permissions != null && this.Permissions.Contains(permission);
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
    }
}
