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
using Lib.data;
using Lib.cache;
using System.Threading.Tasks;
using Hiwjcn.Core.Infrastructure.Auth;
using Hiwjcn.Bll.Auth;
using Hiwjcn.Core.Domain.Auth;
using Lib.events;
using Hiwjcn.Core.Model.Sys;
using Hiwjcn.Framework.Actors;
using Lib.mvc.auth;
using Hiwjcn.Framework;

namespace Hiwjcn.Web.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IAuthLoginService _IAuthLoginService;

        private readonly IAuthTokenService _IAuthTokenService;
        private readonly IAuthScopeService _IAuthScopeService;
        private readonly ICacheProvider _cache;

        private readonly IRepository<AuthScope> _AuthScopeRepository;
        private readonly IRepository<AuthClient> _AuthClientRepository;

        private readonly IAuthTokenToUserService _IAuthTokenToUserService;

        public AuthController(
            IAuthLoginService _IAuthLoginService,
            ICacheProvider _cache,
            IAuthTokenService _IAuthTokenService,
            IAuthScopeService _IAuthScopeService,
            IRepository<AuthScope> _AuthScopeRepository,
            IRepository<AuthClient> _AuthClientRepository,
            IAuthTokenToUserService _IAuthTokenToUserService)
        {
            this._IAuthLoginService = _IAuthLoginService;
            this._cache = _cache;

            this._IAuthTokenService = _IAuthTokenService;
            this._IAuthScopeService = _IAuthScopeService;

            this._AuthScopeRepository = _AuthScopeRepository;
            this._AuthClientRepository = _AuthClientRepository;
            this._IAuthTokenToUserService = _IAuthTokenToUserService;
        }

        /// <summary>
        /// 用code换token
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="client_secret"></param>
        /// <param name="code"></param>
        /// <param name="grant_type"></param>
        /// <returns></returns>
        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> AccessToken(
            string client_id, string client_secret, string code, string grant_type)
        {
            return await RunActionAsync(async () =>
            {
                var data = await this._IAuthTokenService.CreateTokenAsync(client_id, client_secret, code);
                if (ValidateHelper.IsPlumpString(data.msg))
                {
                    return GetJsonRes(data.msg);
                }
                var token_data = new TokenModel()
                {
                    Token = data.data.UID,
                    RefreshToken = data.data.RefreshToken,
                    Expire = data.data.ExpiryTime
                };
                return GetJson(new _() { success = true, data = token_data });
            });
        }

        /// <summary>
        /// 检查token 返回用户信息
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> CheckToken(string client_id, string access_token)
        {
            return await RunActionAsync(async () =>
            {
                var loginuser = await this._IAuthTokenToUserService.FindUserByTokenAsync(access_token, client_id);

                if (!loginuser.success)
                {
                    return GetJsonRes(loginuser.msg);
                }

                return GetJson(new _() { success = true, data = loginuser.data });
            });
        }

        /// <summary>
        /// 用账户密码换取token
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="scope"></param>
        /// <param name="phone"></param>
        /// <param name="sms"></param>
        /// <returns></returns>
        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> AuthCodeByOneTimeCode(string client_id, string scope, string phone, string sms)
        {
            return await RunActionAsync(async () =>
            {
                var loginuser = await this._IAuthLoginService.LoginByCode(phone, sms);
                if (ValidateHelper.IsPlumpString(loginuser.msg))
                {
                    return GetJsonRes(loginuser.msg);
                }
                var scopeslist = scope?.JsonToEntity<List<string>>();
                if (!ValidateHelper.IsPlumpList(scopeslist))
                {
                    scopeslist = (await this._AuthScopeRepository.GetListAsync(null)).Select(x => x.Name).ToList();
                }

                var code = await this._IAuthTokenService.CreateCodeAsync(client_id, scopeslist, loginuser.data.UserID);
                if (ValidateHelper.IsPlumpString(code.msg))
                {
                    return GetJsonRes(code.msg);
                }
                return GetJson(new _() { success = true, data = code.data?.UID });
            });
        }
    }
}