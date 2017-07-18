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

        public virtual string LoginToken { get; set; }

        public virtual List<string> Permissions { get; set; }

        public virtual Dictionary<string, object> ExtraData { get; set; }

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
                e.AddLog("获取登录额外信息错误");
                return default(T);
            }
        }

        /// <summary>
        /// 判断用户是否有权限
        /// </summary>
        /// <param name="permission"></param>
        /// <returns></returns>
        [Obsolete("【请在ioc中配置IFetchUserPermissions】不然这里很可能使用的缓存的权限，而不是及时获取权限！！！")]
        public bool HasPermission(string permission)
        {
            this.LoadNewPermission();
            return this.Permissions != null && this.Permissions.Contains(permission);
        }

        private bool IsNewPermission = false;

        /// <summary>
        /// 加载新权限
        /// </summary>
        private void LoadNewPermission()
        {
            //加载用户权限并缓存到httpcontext中
            try
            {
                //防止多次加载
                if (this.IsNewPermission) { return; }

                Func<List<string>> fetch_func = () => AppContext.GetObject<IFetchUserPermissions>().FetchPermission(this);

                var fresh_permissions = default(List<string>);

                if (ServerHelper.IsHosted())
                {
                    var list = ServerHelper.CacheInHttpContext($"user_permission_during_httprequest:{this.UserID}", fetch_func);
                    fresh_permissions = ConvertHelper.NotNullList(list);
                }
                else
                {
                    fresh_permissions = fetch_func();
                }

                //加入新权限
                this.Permissions = fresh_permissions;
            }
            catch (Exception e)
            {
                e.AddErrorLog("获取用户权限错误，将使用session中存储的权限");
            }
            finally
            {
                this.IsNewPermission = true;
            }
        }

        /// <summary>
        /// 重新加载新权限
        /// </summary>
        public void RefreshPermissions()
        {
            this.IsNewPermission = false;
            this.LoadNewPermission();
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
