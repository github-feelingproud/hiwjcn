using Lib.cache;
using Lib.data.ef;
using Lib.extension;
using Lib.helper;
using Lib.infrastructure.entity.auth;
using Lib.infrastructure.extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lib.auth.provider
{
    public partial class AuthApiServiceFromDbBase<TokenBase> : IAuthApi
        where TokenBase : AuthTokenBase, new()
    {
        protected readonly ICacheProvider _cache;
        protected readonly ICacheKeyManager _keyManager;
        protected readonly IUserLoginApi _login;
        protected readonly IEFRepository<TokenBase> _tokenRepo;

        public AuthApiServiceFromDbBase(
            ICacheProvider _cache,
            ICacheKeyManager _keyManager,
            IUserLoginApi _login,
            IEFRepository<TokenBase> _tokenRepo)
        {
            this._cache = _cache;
            this._keyManager = _keyManager;
            this._login = _login;
            this._tokenRepo = _tokenRepo;
        }

        protected virtual Task CacheHitLog(string cache_key, CacheHitStatusEnum status) => Task.CompletedTask;

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

            this._cache.Remove(this._keyManager.AuthUserInfoCacheKey(token.UserUID));

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
            var cache_key = this._keyManager.AuthTokenCacheKey(access_token);

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
            cache_key = this._keyManager.AuthUserInfoCacheKey(token.UserUID);
            //查找用户
            var loginuser = await this._cache.GetOrSetAsync(cache_key, async () =>
            {
                hit_status = CacheHitStatusEnum.NotHit;

                var user = await this._login.GetLoginUserInfoByUserUID(token.UserUID);

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

        public virtual async Task<_<string>> RemoveCacheAsync(CacheBundle data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("缓存key参数为空");
            }

            var res = new _<string>();

            var keys = new List<string>();

            keys.AddWhenNotEmpty(data.UserUID?.Select(x => this._keyManager.AuthUserInfoCacheKey(x)));
            keys.AddWhenNotEmpty(data.TokenUID?.Select(x => this._keyManager.AuthTokenCacheKey(x)));

            foreach (var key in keys.Distinct())
            {
                this._cache.Remove(key);
            }

            await Task.FromResult(1);

            res.SetSuccessData(string.Empty);

            return res;
        }
    }

    public partial class AuthApiServiceFromDbBase<TokenBase>
    {
        public async Task<_<string>> DeleteUserTokensAsync(string user_uid)
        {
            var data = new _<string>();
            if (!ValidateHelper.IsPlumpString(user_uid))
            {
                data.SetErrorMsg("用户ID为空");
                return data;
            }

            var max_count = 3000;

            var token_to_delete = await this._tokenRepo.GetListAsync(x => x.UserUID == user_uid, count: max_count);

            var errors = await this.DeleteTokensAndRemoveCacheAsync(token_to_delete);
            if (ValidateHelper.IsPlumpString(errors))
            {
                data.SetErrorMsg(errors);
                return data;
            }
            else
            {
                if (max_count == token_to_delete.Count)
                {
                    data.SetErrorMsg("要删除的记录数比较多，请多试几次，直到完全删除");
                    return data;
                }
            }
            data.SetSuccessData(string.Empty);
            return data;
        }

        private async Task<string> DeleteTokensAndRemoveCacheAsync(List<TokenBase> list)
        {
            if (!ValidateHelper.IsPlumpList(list))
            {
                return string.Empty;
            }
            var msg = string.Empty;
            await this._tokenRepo.PrepareSessionAsync(async db =>
            {
                var token_query = db.Set<TokenBase>();

                var token_uid_list = list.Select(x => x.UID).Distinct().ToList();
                var user_uid_list = list.Select(x => x.UserUID).Distinct().ToList();

                token_query.RemoveRange(token_query.Where(x => token_uid_list.Contains(x.UID)));

                if (await db.SaveChangesAsync() <= 0)
                {
                    msg = "删除token失败";
                    return;
                }

                foreach (var token in token_uid_list)
                {
                    this._cache.Remove(this._keyManager.AuthTokenCacheKey(token));
                }
                foreach (var user_uid in user_uid_list)
                {
                    this._cache.Remove(this._keyManager.AuthUserInfoCacheKey(user_uid));
                }
            });
            return msg;
        }

        private async Task RefreshToken(TokenBase tk)
        {
            var now = DateTime.Now;
            var token = await this._tokenRepo.GetFirstAsync(x => x.UID == tk.UID);
            if (token == null || token.ExpiryTime < now || token.IsRemove > 0)
            {
                return;
            }

            //更新过期时间
            token.ExpiryTime = now.AddDays(TokenConfig.TokenExpireDays);
            token.UpdateTime = now;
            token.RefreshTime = now;

            await this._tokenRepo.UpdateAsync(token);
        }

        public virtual async Task<TokenBase> FindTokenAsync(string token_uid)
        {
            var now = DateTime.Now;
            var token = await this._tokenRepo.GetFirstAsync(x => x.UID == token_uid);

            if (token == null || token.ExpiryTime < now)
            {
                return null;
            }

            //自动刷新过期时间
            if ((token.ExpiryTime - now).TotalDays < (TokenConfig.TokenExpireDays / 2.0))
            {
                await this.RefreshToken(token);
            }
            return token;
        }

        public async Task<_<TokenBase>> CreateTokenAsync(string user_uid)
        {
            var data = new _<TokenBase>();
            var now = DateTime.Now;

            //create new token
            var token = new TokenBase()
            {
                ExpiryTime = now.AddDays(TokenConfig.TokenExpireDays),
                RefreshToken = Com.GetUUID(),
                UserUID = user_uid
            }.InitSelf("token");

            if (!token.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await this._tokenRepo.AddAsync(token) <= 0)
            {
                data.SetErrorMsg("保存token失败");
                return data;
            }

            data.SetSuccessData(token);
            return data;
        }
    }
}
