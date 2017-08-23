using Hiwjcn.Core.Infrastructure.User;
using Hiwjcn.Framework;
using Lib.core;
using Lib.helper;
using Lib.ioc;
using Lib.mvc;
using Lib.mvc.user;
using Model.User;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebLogic.Bll.User;
using System.Linq;
using WebLogic.Model.User;
using Lib.data;
using Lib.extension;
using Hiwjcn.Core.Domain.Auth;
using Lib.mvc.auth;
using Lib.cache;
using WebCore.MvcLib.Controller;
using Hiwjcn.Core.Infrastructure.Auth;
using Lib.mvc.auth.validation;
using Hiwjcn.Core.Data;

namespace Hiwjcn.Web.Controllers
{
    public class SSOController : BaseController
    {
        public static readonly LoginStatus loginStatus = new LoginStatus();

        private static LoginUserInfo ReadUser(string uid, string token)
        {
            var loginuser = AppContext.Scope(s =>
            {
                var cache = s.Resolve_<ICacheProvider>();
                var key = $"sso.user.uid={uid}".WithCacheKeyPrefix();
                //读取用户
                var user = cache.GetOrSet(key, () =>
                {
                    using (var db = new SSODB())
                    {
                        return db.T_UserInfo.Where(x => x.UID == uid).FirstOrDefault();
                    }
                }, TimeSpan.FromSeconds(60));
                if (user == null)
                {
                    $"sso用户不存在,uid={uid}".AddBusinessInfoLog();
                    return null;
                }
                if (user.Token() != token)
                {
                    $"sso用户token不正确，uid={uid},token={token}".AddBusinessInfoLog();
                    return null;
                }
                //读取权限
                key = $"sso.user.permissions.uid={uid}".WithCacheKeyPrefix();
                var permissions = cache.GetOrSet(key, () => { }, TimeSpan.FromSeconds(60));
                return new LoginUserInfo();
            });
            return loginuser;
        }

        public static LoginUserInfo GetLoginSSO()
        {
            var context = System.Web.HttpContext.Current;
            var uid = loginStatus.GetCookieUID(context);
            var token = loginStatus.GetCookieToken(context);
            if (ValidateHelper.IsAllPlumpString(uid, token))
            {
                var loginuser = ReadUser(uid, token);
                if (loginuser != null)
                {
                    return loginuser;
                }
            }

            loginStatus.SetUserLogout(context);
            return null;
        }

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> LoginAction(string uname, string pwd)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return null;
            });
        }

        [RequestLog]
        public ActionResult Login(string url, string @continue, string next, string callback)
        {

            return View();
        }

        [RequestLog]
        public async Task<ActionResult> Logout(string url, string @continue, string next, string callback)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);

                loginStatus.SetUserLogout(this.X.context);

                url = Com.FirstPlumpStrOrNot(url, @continue, next, callback, "/");
                return Redirect(url);
            });
        }
    }

    public class SSOPageValidAttribute : ValidLoginBaseAttribute
    {
        protected override LoginUserInfo GetLoginUser(ActionExecutingContext filterContext)
        {
            return SSOController.GetLoginSSO();
        }

        public override void WhenNoPermission(ref ActionExecutingContext filterContext)
        {
            throw new NotImplementedException();
        }

        public override void WhenNotLogin(ref ActionExecutingContext filterContext)
        {
            throw new NotImplementedException();
        }
    }

    public class SSOApiValidAttribute : ValidLoginBaseAttribute
    {
        protected override LoginUserInfo GetLoginUser(ActionExecutingContext filterContext)
        {
            return SSOController.GetLoginSSO();
        }

        public override void WhenNoPermission(ref ActionExecutingContext filterContext)
        {
            throw new NotImplementedException();
        }

        public override void WhenNotLogin(ref ActionExecutingContext filterContext)
        {
            throw new NotImplementedException();
        }
    }
}