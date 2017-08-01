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

        public AuthController(
            IAuthLoginService _IAuthLoginService,
            ICacheProvider _cache,
            IAuthTokenService _IAuthTokenService,
            IAuthScopeService _IAuthScopeService,
            IRepository<AuthScope> _AuthScopeRepository,
            IRepository<AuthClient> _AuthClientRepository)
        {
            this._IAuthLoginService = _IAuthLoginService;
            this._cache = _cache;

            this._IAuthTokenService = _IAuthTokenService;
            this._IAuthScopeService = _IAuthScopeService;

            this._AuthScopeRepository = _AuthScopeRepository;
            this._AuthClientRepository = _AuthClientRepository;
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
                return GetJson(new _() { success = true, data = data.token });
            });
        }

        private static readonly List<bool> HitLogProbability = new List<bool>() { true, false };
        private static readonly Random ran = new Random((int)DateTime.Now.Ticks);

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> CheckToken(string client_id, string access_token)
        {
            return await RunActionAsync(async () =>
            {
                var hit_status = CacheHitStatusEnum.Hit;

                var cache_key = AuthCacheKeyManager.TokenKey(access_token);

                //查找token
                var token = await this._cache.GetOrSetAsync(cache_key, async () =>
                     {
                         hit_status = CacheHitStatusEnum.NotHit;

                         return await this._IAuthTokenService.FindTokenAsync(client_id, access_token);
                     }, TimeSpan.FromMinutes(3));

                if (ran.Choice(HitLogProbability))
                {
                    AkkaHelper<CacheHitLogActor>.Tell(new CacheHitLog()
                    {
                        CacheKey = cache_key,
                        Hit = (int)hit_status
                    });
                }

                if (token == null)
                {
                    return GetJsonRes("token不存在");
                }

                hit_status = CacheHitStatusEnum.Hit;
                cache_key = AuthCacheKeyManager.UserInfoKey(token.UserUID);
                //查找用户
                var loginuser = await this._cache.GetOrSetAsync(cache_key, async () =>
                {
                    hit_status = CacheHitStatusEnum.NotHit;

                    return await this._IAuthLoginService.GetUserInfoByUID(token.UserUID);
                }, TimeSpan.FromMinutes(3));

                if (ran.Choice(HitLogProbability))
                {
                    AkkaHelper<CacheHitLogActor>.Tell(new CacheHitLog()
                    {
                        CacheKey = cache_key,
                        Hit = (int)hit_status
                    });
                }

                if (loginuser == null)
                {
                    return GetJsonRes("用户不存在");
                }

                loginuser.LoginToken = token.UID;
                loginuser.RefreshToken = token.RefreshToken;
                loginuser.TokenExpire = token.ExpiryTime;
                loginuser.Scopes = token.Scopes?.Select(x => x.Name).ToList();

                return GetJson(new _() { success = true, data = loginuser });
            });
        }

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
                var code = await this._IAuthTokenService.CreateCodeAsync(client_id, scope?.JsonToEntity<List<string>>(), loginuser.data.UserID);
                if (ValidateHelper.IsPlumpString(code.msg))
                {
                    return GetJsonRes(code.msg);
                }
                return GetJson(new _() { success = true, data = code.code });
            });
        }
    }
}