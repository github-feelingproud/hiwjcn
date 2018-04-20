using Hiwjcn.Core;
using Hiwjcn.Core.Domain.User;
using Hiwjcn.Framework;
using Hiwjcn.Service.MemberShip;
using Lib.cache;
using Lib.extension;
using Lib.helper;
using Lib.mvc;
using Lib.mvc.auth;
using Lib.mvc.auth.validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Hiwjcn.Web.Controllers
{
    public class AccountController : EpcBaseController
    {
        private readonly IAuthApi _authApi;
        private readonly IUserLoginService _login;
        private readonly ICacheProvider _cache;

        public AccountController(
            IAuthApi _authApi,
            IUserLoginService _login,
            ICacheProvider _cache)
        {
            this._login = _login;
            this._authApi = _authApi;
            this._cache = _cache;
        }

        #region 登录

        [NonAction]
        private async Task<_<T>> AntiRetry<T>(string user_name, Func<Task<_<T>>> func)
        {
            if (!ValidateHelper.IsPlumpString(user_name)) { throw new Exception("username为空"); }

            var cache_key = $"login.retry.count.{user_name}".WithCacheKeyPrefix();
            var expire = 5;
            var max_error = 3;
            var now = DateTime.Now;

            var list = this._cache.Get<List<DateTime>>(cache_key).Result ?? new List<DateTime>() { };
            list = list.Where(x => x > now.AddMinutes(-expire)).ToList();

            var res = new _<T>();

            try
            {
                if (list.Count > max_error)
                {
                    res.SetErrorMsg("错误尝试过多");
                    return res;
                }
                //执行登录
                var data = await func.Invoke();
                if (data.error)
                {
                    list.Add(now);
                }
                return data;
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
                if (!ValidateHelper.IsAllPlumpString(user_name, password))
                {
                    return GetJsonRes("用户名密码不能为空");
                }

                var data = await this.AntiRetry(user_name, async () =>
                {
                    var res = new _<object>();
                    var user = await this._authApi.ValidUserByPasswordAsync(user_name, password);
                    if (user.error)
                    {
                        res.SetErrorMsg(user.msg);
                        return res;
                    }
                    var token = await this._authApi.CreateAccessTokenAsync(user.data.UserID);
                    if (token.error)
                    {
                        res.SetErrorMsg(token.msg);
                        return res;
                    }
                    //reload user
                    user = await this._authApi.GetLoginUserInfoByTokenAsync(token.data.Token);
                    if (user.error)
                    {
                        res.SetErrorMsg(user.msg);
                        return res;
                    }
                    var loginuser = user.data;
                    loginuser.LoginToken = token.data.Token;
                    this.X.context.CookieLogin(loginuser);
                    var token_data = new _()
                    {
                        success = true,
                        data = new
                        {
                            user_uid = user.data.UserID,
                            user_img = user.data.HeadImgUrl,
                            user_name = user.data.NickName,
                            token = token.data.Token,
                        }
                    };
                    res.SetSuccessData(token_data);
                    return res;
                });
                if (data.error)
                {
                    return GetJsonRes(data.msg);
                }
                return GetJson(data.data);
            });
        }

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> LoginByOneTimeCode(string phone, string code)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                throw new NotImplementedException();
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
                var loginuser = await this.GetLoginUserAsync();
                return GetJson(new _() { success = true, data = loginuser });
            });
        }

        [HttpGet]
        public async Task<ActionResult> LoginUser() => await this.GetLoginUserInfo();

        [HttpPost]
        public async Task<ActionResult> Reg(string data)
        {
            return await RunActionAsync(async () =>
            {
                var loginuser = await this.GetLoginUserInfo();
                if (loginuser != null)
                {
                    return GetJsonRes("已经登录，不能注册");
                }

                var user = this.JsonToEntity_<UserEntity>(data);

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
