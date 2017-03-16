using Lib.core;
using Lib.helper;
using System;
using System.Web;
using System.Web.SessionState;

namespace Lib.mvc.user
{
    /// <summary>
    /// 登录状态存取
    /// </summary>
    public class LoginStatus : IRequiresSessionState
    {
        //COOKIE
        public string COOKIE_LOGIN_UID { get; private set; }
        //TOKEN
        public string COOKIE_LOGIN_TOKEN { get; private set; }
        //SESSION
        public string LOGIN_USER_SESSION { get; private set; }
        //DOMAIN
        public string COOKIE_DOMAIN { get; private set; }
        //cookie过期的时间
        public int CookieExpiresMinutes { get; private set; }

        public LoginStatus() : this("USER_UID", "USER_TOKEN", "LOGIN_USER_SESSION", ConfigHelper.Instance.CookieDomain)
        { }

        public LoginStatus(string uid, string token, string session, string domain)
        {
            this.COOKIE_LOGIN_UID = uid;
            this.COOKIE_LOGIN_TOKEN = token;
            this.COOKIE_DOMAIN = domain;
            if (this.CookieExpiresMinutes <= 0)
            {
                this.CookieExpiresMinutes = ConfigHelper.Instance.CookieExpiresMinutes;
            }

            this.LOGIN_USER_SESSION = session;
        }

        /// <summary>
        /// 获取用户cookie的登陆账号和密码
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetCookieUID(HttpContext context = null)
        {
            context = GetContext(context);

            return CookieHelper.GetCookie(context, COOKIE_LOGIN_UID);
        }

        public string GetCookieToken(HttpContext context = null)
        {
            context = GetContext(context);

            return CookieHelper.GetCookie(context, COOKIE_LOGIN_TOKEN);
        }

        /// <summary>
        /// 登陆信息保存到session和cookie
        /// </summary>
        /// <param name="context"></param>
        /// <param name="loginuser"></param>
        /// <param name="SaveCookie"></param>
        /// <returns></returns>
        public void SetUserLogin(HttpContext context = null, LoginUserInfo loginuser = null)
        {
            context = GetContext(context);
            if (loginuser == null) { throw new Exception("登陆状态为空"); }
            if (CookieExpiresMinutes <= 0) { throw new Exception("cookie过期时间必须大于0，请修改配置"); }

            if (!ValidateHelper.IsPlumpString(loginuser.UserID))
            {
                throw new Exception("记录登录状态失败，缺少userid");
            }
            if (!ValidateHelper.IsPlumpString(loginuser.LoginToken))
            {
                throw new Exception("记录登录状态失败，缺少token");
            }

            //保存到session
            SessionHelper.SetSession(context.Session, LOGIN_USER_SESSION, loginuser);
            //保存到cookie
            if (GetCookieUID() != loginuser.UserID)
            {
                CookieHelper.SetCookie(context, COOKIE_LOGIN_UID, loginuser.UserID, domain: COOKIE_DOMAIN,
                        expires_minutes: CookieExpiresMinutes);
            }
            if (GetCookieToken() != loginuser.LoginToken)
            {
                CookieHelper.SetCookie(context, COOKIE_LOGIN_TOKEN, loginuser.LoginToken, domain: COOKIE_DOMAIN,
                    expires_minutes: CookieExpiresMinutes);
            }
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="context"></param>
        public void SetUserLogout(HttpContext context = null)
        {
            context = GetContext(context);

            SessionHelper.RemoveSession(context.Session, LOGIN_USER_SESSION);
            //清空其他cookie操作
            //CookieHelper.RemoveResponseCookies(context, new string[] { COOKIE_LOGIN_UID, COOKIE_LOGIN_TOKEN });
            if (ValidateHelper.IsPlumpString(COOKIE_DOMAIN))
            {
                //删除带域名的cookie
                DeleteCookie(context, true);
            }
            else
            {
                //删除不带域名的cookie
                DeleteCookie(context, false);
            }
        }

        /// <summary>
        /// 如果有域名就删除没有域名的cookie，如果没有域名就删除有域名的cookie
        /// </summary>
        /// <param name="context"></param>
        public void DeleteExtraCookie(HttpContext context = null)
        {
            context = GetContext(context);

            if (ValidateHelper.IsPlumpString(COOKIE_DOMAIN))
            {
                DeleteCookie(context, false);
            }
            else
            {
                DeleteCookie(context, true);
            }
        }

        /// <summary>
        /// 删除cookie
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cookies_with_domain"></param>
        public void DeleteCookie(HttpContext context = null, bool cookies_with_domain = true)
        {
            context = GetContext(context);

            if (cookies_with_domain)
            {
                CookieHelper.RemoveCookie(context, new string[] { COOKIE_LOGIN_UID, COOKIE_LOGIN_TOKEN }, domain: COOKIE_DOMAIN);
            }
            else
            {
                CookieHelper.RemoveCookie(context, new string[] { COOKIE_LOGIN_UID, COOKIE_LOGIN_TOKEN });
            }
        }

        /// <summary>
        /// 获取用户登录实例
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public LoginUserInfo GetLoginUser(HttpContext context = null)
        {
            context = GetContext(context);

            var model = SessionHelper.GetSession<LoginUserInfo>(context.Session, LOGIN_USER_SESSION);
            var cookie_uid = GetCookieUID(context);
            var cookie_token = GetCookieToken(context);

            if (model != null && model.UserID == cookie_uid && model.LoginToken == cookie_token)
            {
                return model;
            }
            return null;
        }

        /// <summary>
        /// 获取上下文
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private HttpContext GetContext(HttpContext _context)
        {
            if (_context != null) { return _context; }
            var context = System.Web.HttpContext.Current;
            if (context == null) { throw new Exception("无法获取上下文对象"); }
            return context;
        }
    }

    /// <summary>
    /// 登录状态存取工厂
    /// </summary>
    public static class AccountHelper
    {
        /// <summary>
        /// 在请求上下文中缓存对象
        /// </summary>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        private static LoginStatus CacheInstance(string key, Func<LoginStatus> func)
        {
            return ServerHelper.CacheInHttpContext(key, func);
        }

        private static readonly string domain = ConfigHelper.Instance.CookieDomain;

        /// <summary>
        /// 用户
        /// </summary>
        public static LoginStatus User
        {
            get
            {
                return CacheInstance(nameof(User), () =>
                {
                    return new LoginStatus();
                });
            }
        }

        /// <summary>
        /// SSO
        /// </summary>
        public static LoginStatus SSO
        {
            get
            {
                return CacheInstance(nameof(Trader), () =>
                {
                    return new LoginStatus("SSO_UID", "SSO_TOKEN", "SSO_SESSION", domain);
                });
            }
        }

        /// <summary>
        /// 卖家
        /// </summary>
        public static LoginStatus Trader
        {
            get
            {
                return CacheInstance(nameof(Trader), () =>
                {
                    return new LoginStatus("TRADER_UID", "TRADER_TOKEN", "TRADER_SESSION", domain);
                });
            }
        }

        /// <summary>
        /// 卖家
        /// </summary>
        public static LoginStatus Seller
        {
            get
            {
                return CacheInstance(nameof(Seller), () =>
                {
                    return new LoginStatus("SELLER_UID", "SELLER_TOKEN", "SELLER_SESSION", domain);
                });
            }
        }

        /// <summary>
        /// 管理员
        /// </summary>
        public static LoginStatus Admin
        {
            get
            {
                return CacheInstance(nameof(Admin), () =>
                {
                    return new LoginStatus("ADMIN_UID", "ADMIN_TOKEN", "ADMIN_SESSION", domain);
                });
            }
        }
    }
}
