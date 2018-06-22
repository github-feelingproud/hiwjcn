using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;
using Lib.data;
using Lib.cache;
using Lib.mvc;
using Lib.helper;
using Lib.extension;
using System.Data.Entity;
using Lib.core;
using Lib.infrastructure.entity.auth;
using Lib.data.ef;
using Lib.infrastructure.extension;
using Lib.infrastructure.model;

namespace Lib.infrastructure.service.user
{
    /// <summary>
    /// auth相关帮助类
    /// </summary>
    public interface IAuthServiceBase<TokenBase>
    {
        Task<_<string>> DeleteUserTokensAsync(string user_uid);

        Task<TokenBase> FindTokenAsync(string token_uid);

        Task<_<TokenBase>> CreateTokenAsync(string user_uid);
    }

    public abstract class AuthServiceBase<TokenBase> :
        IAuthServiceBase<TokenBase>
        where TokenBase : AuthTokenBase, new()
    {
        public Action<string> ClearTokenCacheCallback { get; set; }
        public Action<string> ClearUserTokenCallback { get; set; }

        protected readonly IEFRepository<TokenBase> _tokenRepo;

        public AuthServiceBase(
            IEFRepository<TokenBase> _tokenRepo)
        {
            this._tokenRepo = _tokenRepo;
        }

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
                    this.ClearTokenCacheCallback?.Invoke(token);
                    //this._cache.Remove(this.AuthTokenCacheKey(token));
                }
                foreach (var user_uid in user_uid_list)
                {
                    this.ClearUserTokenCallback?.Invoke(user_uid);
                    //this._cache.Remove(this.AuthUserInfoCacheKey(user_uid));
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
