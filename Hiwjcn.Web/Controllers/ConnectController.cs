using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lib.mvc;
using Lib.mvc.user;
using Lib.helper;
using Lib.core;
using Lib.extension;
using Lib.net;
using Lib.cache;
using System.Threading.Tasks;
using Hiwjcn.Core.Infrastructure.Auth;
using Hiwjcn.Bll.Auth;
using Hiwjcn.Core.Domain.Auth;
using Lib.mvc.auth;
using Hiwjcn.Framework;
using System.Collections.ObjectModel;

namespace Hiwjcn.Web.Controllers
{
    public interface IAuthUserService
    {
        Task<LoginUserInfo> LoginByPassword();

        Task<LoginUserInfo> LoginByToken();

        Task<LoginUserInfo> LoginByOneTimeCode();

        Task<bool> SendOneTimeCode();

        Task<string[]> GetUserPemission();
    }

    [RoutePrefix("connect")]
    public class ConnectController : BaseController
    {
        private readonly IAuthUserService _IUserService;
        private readonly IAuthTokenService _IAuthTokenService;
        private readonly IAuthScopeService _IAuthScopeService;

        public ConnectController(
            IAuthUserService _IUserService,
            IAuthTokenService _IAuthTokenService,
            IAuthScopeService _IAuthScopeService)
        {
            this._IUserService = _IUserService;
            this._IAuthTokenService = _IAuthTokenService;
            this._IAuthScopeService = _IAuthScopeService;
        }

        private readonly ReadOnlyDictionary<string, string> LoginTypeDict = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()
        {
            ["password"] = "~/Views/Connect/Login.cshtml",
            ["onetimecode"] = "~/Views/Connect/LoginByCode.cshtml",
            //["token"] = "",
        });

        [RequestLog]
        public async Task<ActionResult> Login(string url, string loginType)
        {
            return await RunActionAsync(async () =>
            {
                if (!ValidateHelper.IsPlumpString(url))
                {
                    url = "/";
                }

                var loginuser = await this.X.context.GetAuthUserAsync();
                if (loginuser != null)
                {
                    return Redirect(url);
                }

                if (!ValidateHelper.IsPlumpString(loginType))
                {
                    loginType = "password";
                }

                if (!LoginTypeDict.ContainsKey(loginType))
                {
                    return Content("不支持的登录方式");
                }

                return View(LoginTypeDict[loginType]);
            });
        }

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> LoginByPassword(string username, string password)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return View();
            });
        }

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> LoginByToken()
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return View();
            });
        }

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> LoginByOneTimeCode(string phone, string code)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return View();
            });
        }

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> SendOneTimeCode(string phone)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return View();
            });
        }
        
        [RequestLog]
        public async Task<ActionResult> Authorize(string client_id, string redirect_uri, string scope,
            string response_type, string state)
        {
            return await RunActionAsync(async () =>
            {
                var scopes = ConvertHelper.GetString(scope).Trim().Split(',').ToList();
                var scopelist = await this._IAuthScopeService.GetScopesOrDefault(scopes.ToArray());
                ViewData["scopes"] = scopelist;

                var loginuser = await this.X.context.GetAuthUserAsync();


                return View();
            });
        }

        [HttpPost]
        [ApiAuth]
        [RequestLog]
        public async Task<ActionResult> CreateAuthorizeCode(string client_id, List<string> scope)
        {
            return await RunActionAsync(async () =>
            {
                var loginuser = await this.X.context.GetAuthUserAsync();

                var data = await this._IAuthTokenService.CreateCodeAsync(client_id, scope, loginuser.UserID);
                if (ValidateHelper.IsPlumpString(data.msg))
                {
                    return GetJsonRes(data.msg);
                }
                return GetJson(new _() { success = true, data = data.code });
            });
        }

        [ActionName("user_js")]
        [RequestLog]
        public async Task<ActionResult> LoginUser()
        {
            return await RunActionAsync(async () =>
            {
                var loginuser = await this.X.context.GetAuthUserAsync();
                await Task.FromResult(1);
                return View();
            });
        }
    }
}