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
using Lib.mvc.auth.validation;

namespace Hiwjcn.Framework.Provider
{

    /// <summary>
    /// 查询本地库
    /// </summary>
    public class AuthLocalValidationProvider : TokenValidationProviderBase
    {
        private readonly IValidationDataProvider _dataProvider;

        private readonly IAuthLoginService _IAuthLoginService;

        private readonly IAuthTokenService _IAuthTokenService;
        private readonly ICacheProvider _cache;

        public AuthLocalValidationProvider(
            IValidationDataProvider _dataProvider,
            IAuthLoginService _IAuthLoginService,
            IAuthTokenService _IAuthTokenService,
            ICacheProvider _cache)
        {
            this._dataProvider = _dataProvider;
            this._IAuthLoginService = _IAuthLoginService;
            this._IAuthTokenService = _IAuthTokenService;
            this._cache = _cache;
        }

        public override LoginUserInfo FindUser(HttpContext context)
        {
            return AsyncHelper.RunSync(() => FindUserAsync(context));
        }

        public override async Task<LoginUserInfo> FindUserAsync(HttpContext context)
        {
            try
            {
                var access_token = this._dataProvider.GetToken(context);
                var client_id = this._dataProvider.GetClientID(context);
                if (!ValidateHelper.IsAllPlumpString(access_token, client_id))
                {
                    return null;
                }


                var hit_status = CacheHitStatusEnum.Hit;

                var cache_key = AuthCacheKeyManager.TokenKey(access_token);

                //查找token
                var token = await this._cache.GetOrSetAsync(cache_key, async () =>
                {
                    hit_status = CacheHitStatusEnum.NotHit;

                    return await this._IAuthTokenService.FindTokenAsync(client_id, access_token);
                }, TimeSpan.FromMinutes(3));

                AkkaHelper<CacheHitLogActor>.Tell(new CacheHitLog()
                {
                    CacheKey = cache_key,
                    Hit = (int)hit_status
                });

                if (token == null)
                {
                    return null;
                }

                hit_status = CacheHitStatusEnum.Hit;
                cache_key = AuthCacheKeyManager.UserInfoKey(token.UserUID);
                //查找用户
                var loginuser = await this._cache.GetOrSetAsync(cache_key, async () =>
                {
                    hit_status = CacheHitStatusEnum.NotHit;

                    return await this._IAuthLoginService.GetUserInfoByUID(token.UserUID);
                }, TimeSpan.FromMinutes(3));

                AkkaHelper<CacheHitLogActor>.Tell(new CacheHitLog()
                {
                    CacheKey = cache_key,
                    Hit = (int)hit_status
                });

                if (loginuser == null)
                {
                    return null;
                }

                loginuser.LoginToken = token.UID;
                loginuser.RefreshToken = token.RefreshToken;
                loginuser.TokenExpire = token.ExpiryTime;
                loginuser.Scopes = token.Scopes?.Select(x => x.Name).ToList();

                return loginuser;
            }
            catch (Exception e)
            {
                e.AddErrorLog();
                return null;
            }
        }
    }
}
