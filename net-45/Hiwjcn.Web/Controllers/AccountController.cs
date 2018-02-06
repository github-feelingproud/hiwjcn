using Hiwjcn.Bll.Auth;
using Hiwjcn.Core;
using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Framework;
using Lib.cache;
using Lib.data;
using Lib.helper;
using Lib.infrastructure.service;
using Lib.extension;
using Lib.mvc;
using Lib.mvc.auth;
using Lib.mvc.auth.validation;
using Lib.mvc.user;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebCore.MvcLib.Controller;
using Lib.data.ef;
using Hiwjcn.Core.Domain.User;
using Lib.infrastructure.service.user;
using Lib.infrastructure.extension;
using Hiwjcn.Bll.User;

namespace Hiwjcn.Web.Controllers
{
    public class AccountController : EpcBaseController
    {
        private readonly IReadOnlyCollection<string> DefaultScopes = new List<string>() { }.AsReadOnly();

        private readonly IAuthLoginProvider _IAuthLoginService;
        private readonly IUserLoginService _login;
        private readonly IAuthApi _authApi;
        private readonly IAuthDataProvider _dataProvider;
        private readonly ICacheProvider _cache;

        public AccountController(
            IAuthLoginProvider _IAuthLoginService,
            IUserLoginService _login,
            IAuthApi _authApi,
            IAuthDataProvider _dataProvider,
            ICacheProvider _cache)
        {
            this._IAuthLoginService = _IAuthLoginService;
            this._login = _login;
            this._authApi = _authApi;
            this._dataProvider = _dataProvider;
            this._cache = _cache;
        }

        #region 登录
        [NonAction]
        private string retry_count_cache_key(string key) => $"login.retry.count.{key}".WithCacheKeyPrefix();

        [NonAction]
        private async Task<ActionResult> AntiRetry(string user_name, Func<Task<_<ActionResult>>> func)
        {
            if (!ValidateHelper.IsPlumpString(user_name)) { throw new Exception("username为空"); }

            var cache_key = this.retry_count_cache_key(user_name);
            var expire = 5;
            var max_error = 3;
            var now = DateTime.Now;

            var list = this._cache.Get<List<DateTime>>(cache_key).Result ?? new List<DateTime>() { };
            list = list.Where(x => x > now.AddMinutes(-expire)).ToList();

            try
            {
                if (list.Count > max_error)
                {
                    return GetJsonRes("错误尝试过多");
                }
                //执行登录
                var data = await func.Invoke();
                if (data.error)
                {
                    list.Add(now);
                    return GetJsonRes(data.msg);
                }
                return data.data;
            }
            finally
            {
                this._cache.Set(cache_key, list, TimeSpan.FromMinutes(expire));
            }
        }

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> LoginViaPass(string user_name, string password)
        {
            return await RunActionAsync(async () =>
            {
                return await this.AntiRetry(user_name, async () =>
                {
                    var res = new _<ActionResult>();
                    var client_id = this._dataProvider.GetClientID(this.X.context);
                    var client_security = this._dataProvider.GetClientSecurity(this.X.context);
                    var code = await this._authApi.GetAuthCodeByPasswordAsync(client_id, this.DefaultScopes.ToList(), user_name, password);
                    if (code.error)
                    {
                        res.SetErrorMsg(code.msg);
                        return res;
                    }
                    var token = await this._authApi.GetAccessTokenAsync(client_id, client_security, code.data, string.Empty);
                    if (token.error)
                    {
                        res.SetErrorMsg(token.msg);
                        return res;
                    }
                    var user = await this._authApi.GetLoginUserInfoByTokenAsync(client_id, token.data.Token);
                    if (user.error)
                    {
                        res.SetErrorMsg(user.msg);
                        return res;
                    }
                    var loginuser = user.data;
                    loginuser.LoginToken = token.data.Token;
                    this.X.context.CookieLogin(loginuser);
                    res.SetSuccessData(GetJson(new _()
                    {
                        success = true,
                        data = new
                        {
                            user_uid = user.data.UserID,
                            user_img = user.data.HeadImgUrl,
                            user_name = user.data.NickName,
                            token = token.data.Token,
                        }
                    }));
                    return res;
                });
            });
        }

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> LoginByOneTimeCode(string phone, string code)
        {
            return await RunActionAsync(async () =>
            {
                throw new NotImplementedException();
            });
        }

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> SendOneTimeCode(string phone)
        {
            return await RunActionAsync(async () =>
            {
                var data = await this._IAuthLoginService.SendOneTimeCode(phone);

                return GetJsonRes(data.msg);
            });
        }

        /// <summary>
        /// 登录账户
        /// </summary>
        /// <returns></returns>
        [RequestLog]
        public async Task<ActionResult> Login(string url, string @continue, string next, string callback)
        {
            return await RunActionAsync(async () =>
            {
                url = new string[] { url, @continue, next, callback, "/" }.FirstNotEmpty_();

                var auth_user = await this.X.context.GetAuthUserAsync();
                if (auth_user != null)
                {
                    return Redirect(url);
                }

                return View();
            });
        }

        /// <summary>
        /// 退出地址
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult LogOut(string url, string @continue, string next, string callback)
        {
            return RunAction(() =>
            {
                this.X.context.CookieLogout();

                url = new string[] { url, @continue, next, callback, "/" }.FirstNotEmpty_();

                return Redirect(url);
            });
        }

        [HttpPost]
        public ActionResult LogOutAction()
        {
            return RunAction(() =>
            {
                this.X.context.CookieLogout();
                return GetJsonRes(string.Empty);
            });
        }

        [HttpPost]
        public async Task<ActionResult> GetLoginUserInfo()
        {
            return await RunActionAsync(async () =>
            {
                var loginuser = await this.X.context.GetAuthUserAsync();
                return GetJson(new _() { success = loginuser != null, data = loginuser });
            });
        }

        [HttpGet]
        public async Task<ActionResult> LoginUser()
        {
            return await RunActionAsync(async () =>
            {
                var loginuser = await this.X.context.GetAuthUserAsync();

                return GetJsonp(new _() { success = loginuser != null, data = loginuser });
            });
        }

        [HttpPost]
        public async Task<ActionResult> Reg(string data)
        {
            return await RunActionAsync(async () =>
            {
                var user = data?.JsonToEntity<UserEntity>(throwIfException: false);
                if (user == null)
                {
                    return GetJsonRes("参数错误");
                }
                var res = await this._login.RegisterUser(user);
                if (res.error)
                {
                    return GetJsonRes(res.msg);
                }

                return GetJsonRes(string.Empty);
            });
        }

        #endregion
    }
}
