using Lib.cache;
using Lib.data.ef;
using Lib.extension;
using Lib.helper;
using Lib.infrastructure.entity.auth;
using Lib.infrastructure.service.user;
using Lib.mvc;
using Lib.mvc.auth;
using Lib.mvc.user;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lib.infrastructure.provider
{
    public abstract class AuthApiServiceFromDbBase<TokenBase> :
        AuthServiceBase<TokenBase>,
        IAuthApi
        where TokenBase : AuthTokenBase, new()
    {
        protected readonly ICacheProvider _cache;

        public AuthApiServiceFromDbBase(
            ICacheProvider _cache,
            IEFRepository<TokenBase> _tokenRepo) :
            base(_tokenRepo)
        {
            this._cache = _cache;

            this.ClearTokenCacheCallback = x => this._cache.Remove(this.AuthTokenCacheKey(x));
            this.ClearUserTokenCallback = x => this._cache.Remove(this.AuthUserInfoCacheKey(x));
        }

        public abstract Task CacheHitLog(string cache_key, CacheHitStatusEnum status);

        public virtual async Task<_<TokenModel>> CreateAccessTokenAsync(string user_uid)
        {
            var res = new _<TokenModel>();

            var data = await this.CreateTokenAsync(user_uid);
            if (data.error)
            {
                res.SetErrorMsg(data.msg);
                return res;
            }
            var token = data.data;
            var token_data = new TokenModel()
            {
                Token = token.UID,
                RefreshToken = token.RefreshToken,
                Expire = token.ExpiryTime
            };

            this._cache.Remove(this.AuthUserInfoCacheKey(token.UserUID));

            res.SetSuccessData(token_data);
            return res;
        }

        public virtual async Task<_<LoginUserInfo>> GetLoginUserInfoByTokenAsync(string access_token)
        {
            var data = new _<LoginUserInfo>();

            if (!ValidateHelper.IsAllPlumpString(access_token))
            {
                data.SetErrorMsg("参数为空");
                return data;
            }

            var cache_expire = TimeSpan.FromMinutes(10);

            var hit_status = CacheHitStatusEnum.Hit;
            var cache_key = this.AuthTokenCacheKey(access_token);

            //查找token
            var token = await this._cache.GetOrSetAsync(cache_key, async () =>
            {
                hit_status = CacheHitStatusEnum.NotHit;

                return await this.FindTokenAsync(access_token);
            }, cache_expire);

            //统计缓存命中
            await this.CacheHitLog(cache_key, hit_status);

            if (token == null)
            {
                data.SetErrorMsg("token不存在");
                return data;
            }

            hit_status = CacheHitStatusEnum.Hit;
            cache_key = this.AuthUserInfoCacheKey(token.UserUID);
            //查找用户
            var loginuser = await this._cache.GetOrSetAsync(cache_key, async () =>
            {
                hit_status = CacheHitStatusEnum.NotHit;

                var user = await this.GetLoginUserInfoByUserUID(token.UserUID);

                return user;
            }, cache_expire);

            //统计缓存命中
            await this.CacheHitLog(cache_key, hit_status);

            if (loginuser == null)
            {
                data.SetErrorMsg("用户不存在");
                return data;
            }

            loginuser.LoginToken = token.UID;
            loginuser.RefreshToken = token.RefreshToken;
            loginuser.TokenExpire = token.ExpiryTime;

            data.SetSuccessData(loginuser);

            return data;
        }

        public abstract Task<LoginUserInfo> GetLoginUserInfoByUserUID(string uid);

        public abstract Task<_<LoginUserInfo>> ValidUserByOneTimeCodeAsync(string phone, string sms);

        public abstract Task<_<LoginUserInfo>> ValidUserByPasswordAsync(string username, string password);

        public virtual async Task<_<string>> RemoveCacheAsync(CacheBundle data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("缓存key参数为空");
            }

            var res = new _<string>();

            var keys = new List<string>();

            keys.AddWhenNotEmpty(data.UserUID?.Select(x => this.AuthUserInfoCacheKey(x)));
            keys.AddWhenNotEmpty(data.TokenUID?.Select(x => this.AuthTokenCacheKey(x)));

            foreach (var key in keys.Distinct())
            {
                this._cache.Remove(key);
            }

            await Task.FromResult(1);

            res.SetSuccessData(string.Empty);

            return res;
        }

        public abstract string AuthTokenCacheKey(string token);

        public abstract string AuthUserInfoCacheKey(string user_uid);

    }
}
