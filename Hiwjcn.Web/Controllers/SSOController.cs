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
using System.Collections.Generic;
using System.Net;
using System.Data.Entity;
using Hiwjcn.Framework.Provider;

namespace Hiwjcn.Web.Controllers
{
    public class SSOController : BaseController
    {
        public static LoginStatus loginStatus => AccountHelper.SSO;

        private static T_UserInfo GetUser(string uid)
        {
            using (var db = new SSODB())
            {
                var model = db.T_UserInfo.Where(x => x.UID == uid).FirstOrDefault();
                if (model == null)
                {
                    return null;
                }

                //这里只拿了角色关联的权限，部门关联的权限没有拿
                var roleslist = db.Auth_UserRole.Where(x => x.UserID == uid)
                    .Select(x => x.RoleID).ToList()
                    .Select(x => $"role:{x}").ToList();

                model.Permissions = db.Auth_PermissionMap.Where(x => roleslist.Contains(x.MapKey))
                    .Select(x => x.PermissionID).ToList()
                    .Distinct().ToList();

                return model;
            }
        }

        public static LoginUserInfo GetLoginSSO()
        {
            var context = System.Web.HttpContext.Current;

            return context.CacheInHttpContext("sso.loginuser.entity", () =>
            {
                var uid = loginStatus.GetCookieUID(context);
                var token = loginStatus.GetCookieToken(context);
                if (ValidateHelper.IsAllPlumpString(uid, token))
                {
                    var user = AppContext.Scope(s =>
                    {
                        var cache = s.Resolve_<ICacheProvider>();
                        var key = $"sso.user.uid={uid}".WithCacheKeyPrefix();
                        return cache.GetOrSet(key, () => GetUser(uid), TimeSpan.FromSeconds(60));
                    });

                    if (user != null && user.CreateToken() == token)
                    {
                        var loginuser = user.LoginUserInfo();
                        loginStatus.SetUserLogin(context, loginuser);

                        return loginuser;
                    }
                }

                loginStatus.SetUserLogout(context);
                return null;
            });
        }

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> LoginAction(string username, string password)
        {
            return await RunActionAsync(async () =>
            {
                if (!ValidateHelper.IsAllPlumpString(username, password))
                {
                    return GetJsonRes("请输入账号密码");
                }

                using (var db = new SSODB())
                {
                    var md5 = password.ToMD5().ToUpper();
                    var model = await db.T_UserInfo.Where(x => x.UserName == username && x.PassWord == md5).FirstOrDefaultAsync();
                    if (model == null)
                    {
                        return GetJsonRes("账户密码错误");
                    }
                    var user = GetUser(model.UID);
                    if (user == null)
                    {
                        return GetJsonRes("读取用户信息异常");
                    }
                    var loginuser = user.LoginUserInfo();
                    loginStatus.SetUserLogin(this.X.context, loginuser);
                    return GetJsonRes(string.Empty);
                }
            });
        }

        [RequestLog]
        public async Task<ActionResult> Login(string url, string @continue, string next, string callback)
        {
            return await RunActionAsync(async () =>
            {
                url = Com.FirstPlumpStrOrNot(url, @continue, next, callback, "/");
                var loginuser = SSOController.GetLoginSSO();
                if (loginuser != null)
                {
                    return Redirect(url);
                }

                await Task.FromResult(1);
                return View();
            });
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

        [SSOPageValid]
        public ActionResult test()
        {
            return GetJson(SSOController.GetLoginSSO());
        }
    }

    public class SSOPageValidAttribute : ValidLoginBaseAttribute
    {

        private readonly TokenValidationProviderBase valid = new SSOValidationProvider();

        protected override LoginUserInfo GetLoginUser(ActionExecutingContext filterContext)
        {
            return valid.GetLoginUserInfo(HttpContext.Current);
        }

        public override void WhenNoPermission(ref ActionExecutingContext filterContext)
        {
            filterContext.Result = new ViewResult() { ViewName = "~/Views/Shared/Limited.cshtml" };
        }

        public override void WhenNotLogin(ref ActionExecutingContext filterContext)
        {
            var current_url = filterContext.HttpContext.Request.Url.ToString();
            current_url = EncodingHelper.UrlEncode(current_url);
            filterContext.Result = new RedirectResult($"/sso/login?continue={current_url}");
        }
    }

    public class SSOApiValidAttribute : ValidLoginBaseAttribute
    {
        private readonly SSOValidationProvider valid = new SSOValidationProvider();

        protected override LoginUserInfo GetLoginUser(ActionExecutingContext filterContext)
        {
            return valid.GetLoginUserInfo(HttpContext.Current);
        }

        public override void WhenNoPermission(ref ActionExecutingContext filterContext)
        {
            filterContext.Result = GetJson(new _() { success = false, msg = "没有权限", code = (-(int)HttpStatusCode.Unauthorized).ToString() });
        }

        public override void WhenNotLogin(ref ActionExecutingContext filterContext)
        {
            filterContext.Result = GetJson(new _() { success = false, msg = "没有登录", code = (-999).ToString() });
        }
    }
}