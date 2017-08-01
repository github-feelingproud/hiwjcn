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
    [RoutePrefix("connect")]
    public class ConnectController : BaseController
    {
        private readonly IAuthLoginService _IAuthLoginService;

        private readonly IAuthTokenService _IAuthTokenService;
        private readonly IAuthScopeService _IAuthScopeService;
        private readonly IAuthClientService _IAuthClientService;

        public ConnectController(
            IAuthLoginService _IAuthLoginService,
            IAuthTokenService _IAuthTokenService,
            IAuthScopeService _IAuthScopeService,
            IAuthClientService _IAuthClientService)
        {
            this._IAuthLoginService = _IAuthLoginService;

            this._IAuthTokenService = _IAuthTokenService;
            this._IAuthScopeService = _IAuthScopeService;
            this._IAuthClientService = _IAuthClientService;
        }

        private readonly ReadOnlyDictionary<string, string> LoginTypeDict = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()
        {
            ["password"] = "~/Views/Connect/Login.cshtml",
            ["onetimecode"] = "~/Views/Connect/LoginByCode.cshtml",
            //["token"] = "",
        });

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> LoginByPassword(string username, string password)
        {
            return await RunActionAsync(async () =>
            {
                var data = await this._IAuthLoginService.LoginByPassword(username, password);
                if (data.success)
                {
                    this.X.context.CookieLogin(data.data);
                    return GetJsonRes(string.Empty);
                }
                return GetJsonRes(data.msg);
            });
        }

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> LoginByOneTimeCode(string phone, string code)
        {
            return await RunActionAsync(async () =>
            {
                var data = await this._IAuthLoginService.LoginByCode(phone, code);

                if (ValidateHelper.IsPlumpString(data.msg))
                {
                    return GetJsonRes(data.msg);
                }

                this.X.context.CookieLogin(data.data);

                return GetJsonRes(string.Empty);
            });
        }

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> SendOneTimeCode(string phone)
        {
            return await RunActionAsync(async () =>
            {
                var data = await this._IAuthLoginService.SendOneTimeCode(phone);
                return GetJsonRes(data);
            });
        }

        [RequestLog]
        public async Task<ActionResult> Authorize(string client_id, string redirect_uri, string scope,
            string response_type, string state, string login_type, string return_type)
        {
            return await RunActionAsync(async () =>
            {
                if (!ValidateHelper.IsPlumpString(redirect_uri))
                {
                    return Content($"{nameof(redirect_uri)}为空");
                }

                ViewData[nameof(redirect_uri)] = redirect_uri;
                ViewData[nameof(login_type)] = login_type;
                ViewData[nameof(return_type)] = return_type;

                var scopes = ConvertHelper.GetString(scope).Trim().Split(',').Where(x => ValidateHelper.IsPlumpString(x)).ToList();
                var scopelist = await this._IAuthScopeService.GetScopesOrDefault(scopes.ToArray());
                ViewData["scopes"] = scopelist.OrderByDescending(x => x.Important).ToList();

                var client = await this._IAuthClientService.GetByID(client_id);
                if (client == null)
                {
                    return Content("client_id无效");
                }
                ViewData["client"] = client;

                //使用异步加载
                var loginuser = await this.X.context.GetAuthUserAsync();

                return View();
            });
        }

        [HttpPost]
        [ApiAuth]
        [RequestLog]
        public async Task<ActionResult> CreateAuthorizeCode(string client_id, string scope)
        {
            return await RunActionAsync(async () =>
            {
                var loginuser = await this.X.context.GetAuthUserAsync();

                var data = await this._IAuthTokenService.CreateCodeAsync(client_id, scope?.JsonToEntity<List<string>>(), loginuser.UserID);
                if (ValidateHelper.IsPlumpString(data.msg))
                {
                    return GetJsonRes(data.msg);
                }
                return GetJson(new _() { success = true, data = data.code?.UID });
            });
        }

        public ActionResult Logout(string url)
        {
            return RunAction(() =>
            {
                this.X.context.CookieLogout();

                if (!ValidateHelper.IsPlumpString(url))
                {
                    url = "/";
                }

                return Redirect(url);
            });
        }

        [RequestLog]
        public async Task<ActionResult> LoginUser()
        {
            return await RunActionAsync(async () =>
            {
                var loginuser = await this.X.context.GetAuthUserAsync();

                return GetJsonp(new _() { success = loginuser != null, data = loginuser });
            });
        }
    }
}