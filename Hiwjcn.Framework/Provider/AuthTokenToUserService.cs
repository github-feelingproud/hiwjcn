using Hiwjcn.Core.Infrastructure.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.helper;
using Lib.core;
using Lib.cache;
using Lib.extension;
using Lib.mvc.user;
using Hiwjcn.Core.Model.Sys;
using Lib.events;
using Hiwjcn.Framework.Actors;
using Lib.mvc;

namespace Hiwjcn.Bll.Auth
{
    public class AuthTokenToUserService : IAuthTokenToUserService
    {
        private readonly IAuthLoginService _IAuthLoginService;
        private readonly IAuthTokenService _IAuthTokenService;
        private readonly ICacheProvider _cache;
        public AuthTokenToUserService(
            IAuthLoginService _IAuthLoginService,
            IAuthTokenService _IAuthTokenService,
            ICacheProvider _cache)
        {
            this._IAuthLoginService = _IAuthLoginService;
            this._IAuthTokenService = _IAuthTokenService;
            this._cache = _cache;
        }

        public async Task<_<LoginUserInfo>> FindUserByTokenAsync(string access_token, string client_id)
        {
            var data = new _<LoginUserInfo>();

            if (!ValidateHelper.IsAllPlumpString(access_token, client_id))
            {
                data.SetErrorMsg("参数为空");
                return data;
            }

            var hit_status = CacheHitStatusEnum.Hit;
            var cache_expire = TimeSpan.FromMinutes(5);

            var cache_key = AuthCacheKeyManager.TokenKey(access_token);

            //查找token
            var token = await this._cache.GetOrSetAsync(cache_key, async () =>
            {
                hit_status = CacheHitStatusEnum.NotHit;

                return await this._IAuthTokenService.FindTokenAsync(client_id, access_token);
            }, cache_expire);

            AkkaHelper<CacheHitLogActor>.Tell(new CacheHitLog()
            {
                CacheKey = cache_key,
                Hit = (int)hit_status
            });

            if (token == null)
            {
                data.SetErrorMsg("token不存在");
                return data;
            }

            hit_status = CacheHitStatusEnum.Hit;
            cache_key = AuthCacheKeyManager.UserInfoKey(token.UserUID);
            //查找用户
            var loginuser = await this._cache.GetOrSetAsync(cache_key, async () =>
            {
                hit_status = CacheHitStatusEnum.NotHit;

                var user = await this._IAuthLoginService.GetUserInfoByUID(token.UserUID);
                if (user != null)
                {
                    user = await this._IAuthLoginService.LoadPermissions(user);
                }
                return user;
            }, cache_expire);

            AkkaHelper<CacheHitLogActor>.Tell(new CacheHitLog()
            {
                CacheKey = cache_key,
                Hit = (int)hit_status
            });

            if (loginuser == null)
            {
                data.SetErrorMsg("用户不存在");
                return data;
            }

            loginuser.LoginToken = token.UID;
            loginuser.RefreshToken = token.RefreshToken;
            loginuser.TokenExpire = token.ExpiryTime;
            loginuser.Scopes = token.Scopes?.Select(x => x.Name).ToList();

            data.SetSuccessData(loginuser);

            return data;
        }
    }
}
