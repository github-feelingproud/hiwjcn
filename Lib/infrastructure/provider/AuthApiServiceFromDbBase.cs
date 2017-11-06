using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.cache;
using Lib.infrastructure.service;
using Lib.infrastructure.entity;
using Lib.mvc;
using Lib.mvc.user;
using Lib.extension;
using Lib.helper;
using Lib.mvc.auth;
using Lib.data;
using Lib.core;
using System.Data.Entity;

namespace Lib.infrastructure.provider
{
    public abstract class AuthApiServiceFromDbBase<ClientBase, ScopeBase, TokenBase, CodeBase, TokenScopeBase> : IAuthApi
        where ClientBase : AuthClientBase, new()
        where ScopeBase : AuthScopeBase, new()
        where TokenBase : AuthTokenBase, new()
        where CodeBase : AuthCodeBase, new()
        where TokenScopeBase : AuthTokenScopeBase, new()
    {
        private readonly object AuthApiServiceFromDB = new object();

        protected readonly IAuthLoginService _loginService;
        //先不用这个
        //protected readonly IAuthServiceBase<ClientBase, ScopeBase, TokenBase, CodeBase, TokenScopeBase> _authService;
        protected readonly ICacheProvider _cache;

        protected readonly IRepository<ClientBase> _clientRepo;
        protected readonly IRepository<ScopeBase> _scopeRepo;
        protected readonly IRepository<TokenBase> _tokenRepo;
        protected readonly IRepository<CodeBase> _codeRepo;
        protected readonly IRepository<TokenScopeBase> _tokenScopeRepo;

        public AuthApiServiceFromDbBase(
            IAuthLoginService _loginService,
            //IAuthServiceBase<ClientBase, ScopeBase, TokenBase, CodeBase, TokenScopeBase> _authService,
            ICacheProvider _cache,
            IRepository<ClientBase> _clientRepo,
            IRepository<ScopeBase> _scopeRepo,
            IRepository<TokenBase> _tokenRepo,
            IRepository<CodeBase> _codeRepo,
            IRepository<TokenScopeBase> _tokenScopeRepo)
        {
            this._loginService = _loginService;
            //this._authService = _authService;
            this._cache = _cache;
            this._clientRepo = _clientRepo;
            this._scopeRepo = _scopeRepo;
            this._tokenRepo = _tokenRepo;
            this._codeRepo = _codeRepo;
            this._tokenScopeRepo = _tokenScopeRepo;
        }

        public abstract Task CacheHitLog(string cache_key, CacheHitStatusEnum status);
        public abstract string AuthTokenKey(string token);
        public abstract string AuthUserInfoKey(string user_uid);
        public abstract string AuthSSOUserInfoKey(string user_uid);
        public abstract string AuthClientKey(string client);
        public abstract string AuthScopeKey(string scope);


        public async Task<_<TokenModel>> GetAccessTokenAsync(string client_id, string client_secret, string code, string grant_type)
        {
            var res = new _<TokenModel>();

            var func = $"{nameof(AuthApiServiceFromDB)}.{nameof(GetAccessTokenAsync)}";
            var p = new { client_id = client_id, client_secret = client_secret, code = code, grant_type = grant_type }.ToJson();

            var data = await this.CreateTokenAsync(client_id, client_secret, code);
            if (data.error)
            {
                $"获取token异常|{data.msg}|{func}|{p}".AddBusinessInfoLog();

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

            this._cache.Remove(this.AuthUserInfoKey(token.UserUID));

            res.SetSuccessData(token_data);
            return res;
        }

        public async Task<_<LoginUserInfo>> GetLoginUserInfoByTokenAsync(string client_id, string access_token)
        {
            var data = new _<LoginUserInfo>();

            var func = $"{nameof(AuthApiServiceFromDB)}.{nameof(GetLoginUserInfoByTokenAsync)}";
            var p = new { client_id = client_id, access_token = access_token }.ToJson();

            if (!ValidateHelper.IsAllPlumpString(access_token, client_id))
            {
                $"验证token异常|参数为空|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg("参数为空");
                return data;
            }

            var cache_expire = TimeSpan.FromMinutes(10);

            var hit_status = CacheHitStatusEnum.Hit;
            var cache_key = this.AuthTokenKey(access_token);

            //查找token
            var token = await this._cache.GetOrSetAsync(cache_key, async () =>
            {
                hit_status = CacheHitStatusEnum.NotHit;

                return await this.FindTokenAsync(client_id, access_token);
            }, cache_expire);

            //统计缓存命中
            await this.CacheHitLog(cache_key, hit_status);

            if (token == null)
            {
                $"token不存在|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg("token不存在");
                return data;
            }

            hit_status = CacheHitStatusEnum.Hit;
            cache_key = this.AuthUserInfoKey(token.UserUID);
            //查找用户
            var loginuser = await this._cache.GetOrSetAsync(cache_key, async () =>
            {
                hit_status = CacheHitStatusEnum.NotHit;

                return await this._loginService.GetUserInfoByUID(token.UserUID);
            }, cache_expire);

            //统计缓存命中
            await this.CacheHitLog(cache_key, hit_status);

            if (loginuser == null)
            {
                $"用户不存在|{func}|{p}".AddBusinessInfoLog();

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

        public async Task<_<string>> GetAuthCodeByOneTimeCodeAsync(string client_id, List<string> scopes, string phone, string sms)
        {
            var data = new _<string>();

            var func = $"{nameof(AuthApiServiceFromDB)}.{nameof(GetAuthCodeByOneTimeCodeAsync)}";
            var p = new { client_id = client_id, scope = scopes, phone = phone, sms = sms }.ToJson();

            var loginuser = await this._loginService.LoginByCode(phone, sms);
            if (loginuser.error)
            {
                $"验证码登录失败|{loginuser.msg}|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg(loginuser.msg);
                return data;
            }
            var scopeslist = AuthHelper.ParseScopes(scopes);
            if (!ValidateHelper.IsPlumpList(scopeslist))
            {
                scopeslist = (await this._scopeRepo.GetListAsync(null)).Select(x => x.Name).ToList();
            }

            var code = await this.CreateCodeAsync(client_id, scopeslist, loginuser.data.UserID);
            if (code.error)
            {
                $"创建Code失败|{code.msg}|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg(code.msg);
                return data;
            }

            data.SetSuccessData(code.data.UID);
            return data;
        }

        public async Task<_<string>> GetAuthCodeByPasswordAsync(string client_id, List<string> scopes, string username, string password)
        {
            var data = new _<string>();

            var func = $"{nameof(AuthApiServiceFromDB)}.{nameof(GetAuthCodeByPasswordAsync)}";
            var p = new { client_id = client_id, scope = scopes, username = username, password = password }.ToJson();

            var loginuser = await this._loginService.LoginByPassword(username, password);
            if (loginuser.error)
            {
                $"密码登录失败|{loginuser.msg}|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg(loginuser.msg);
                return data;
            }
            var scopeslist = AuthHelper.ParseScopes(scopes);
            if (!ValidateHelper.IsPlumpList(scopeslist))
            {
                scopeslist = (await this._scopeRepo.GetListAsync(null)).Select(x => x.Name).ToList();
            }

            var code = await this.CreateCodeAsync(client_id, scopeslist, loginuser.data.UserID);
            if (code.error)
            {
                $"创建Code失败|{code.msg}|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg(code.msg);
                return data;
            }

            data.SetSuccessData(code.data.UID);
            return data;
        }

        public async Task<_<string>> RemoveCacheAsync(CacheBundle data)
        {
            var res = new _<string>();
            if (data == null)
            {
                res.SetErrorMsg("缓存key参数为空");
                return res;
            }

            var keys = new List<string>();
            if (ValidateHelper.IsPlumpList(data.UserUID))
            {
                keys.AddRange(data.UserUID.Select(x => this.AuthUserInfoKey(x)));
            }
            if (ValidateHelper.IsPlumpList(data.TokenUID))
            {
                keys.AddRange(data.TokenUID.Select(x => this.AuthTokenKey(x)));
            }
            if (ValidateHelper.IsPlumpList(data.SSOUserUID))
            {
                keys.AddRange(data.SSOUserUID.Select(x => this.AuthSSOUserInfoKey(x)));
            }
            if (ValidateHelper.IsPlumpList(data.ClientUID))
            {
                keys.AddRange(data.ClientUID.Select(x => this.AuthClientKey(x)));
            }
            if (ValidateHelper.IsPlumpList(data.ScopeUID))
            {
                keys.AddRange(data.ScopeUID.Select(x => this.AuthScopeKey(x)));
            }

            foreach (var key in keys.Distinct())
            {
                this._cache.Remove(key);
            }

            await Task.FromResult(1);

            res.SetSuccessData(string.Empty);

            return res;
        }

        #region 帮助方法
        private async Task ClearOldCode(string user_uid)
        {
            var expire = DateTime.Now.AddMinutes(-TokenConfig.CodeExpireMinutes);
            await this._codeRepo.DeleteWhereAsync(x => x.UserUID == user_uid && x.CreateTime < expire);
        }

        private async Task<_<CodeBase>> CreateCodeAsync(string client_uid, List<string> scopes, string user_uid)
        {
            var data = new _<CodeBase>();

            if (!ValidateHelper.IsPlumpList(scopes))
            {
                data.SetErrorMsg("scopes为空");
                return data;
            }
            var now = DateTime.Now;

            var client = await this._clientRepo.GetFirstAsync(x => x.UID == client_uid && x.IsRemove <= 0);
            if (client == null)
            {
                data.SetErrorMsg("授权的应用已被删除");
                return data;
            }

            var border = now.GetDateBorder();

            var count = await this._codeRepo.GetCountAsync(x =>
              x.ClientUID == client_uid &&
              x.UserUID == user_uid &&
              x.CreateTime >= border.start &&
              x.CreateTime < border.end);

            if (count >= TokenConfig.MaxCodeCreatedDaily)
            {
                data.SetErrorMsg("授权过多");
                return data;
            }

            var code = new CodeBase()
            {
                UserUID = user_uid,
                ClientUID = client_uid,
                ScopesJson = scopes.ToJson()
            };
            code.Init("code");

            if (await this._codeRepo.AddAsync(code) > 0)
            {
                //clear old data
                await this.ClearOldCode(user_uid);

                data.SetSuccessData(code);
                return data;
            }
            throw new MsgException("保存票据失败");
        }
        private async Task ClearOldToken(string user_uid)
        {
            var now = DateTime.Now;
            await this._tokenRepo.DeleteWhereAsync(x => x.UserUID == user_uid && x.ExpiryTime < now);
        }

        private async Task<_<TokenBase>> CreateTokenAsync(
            string client_id, string client_secret, string code_uid)
        {
            var data = new _<TokenBase>();
            var now = DateTime.Now;

            //code
            var expire = now.AddMinutes(-TokenConfig.CodeExpireMinutes);
            var code = await this._codeRepo.GetFirstAsync(x =>
            x.UID == code_uid &&
            x.ClientUID == client_id &&
            x.CreateTime > expire &&
            x.IsRemove <= 0);
            if (code == null)
            {
                data.SetErrorMsg("code已失效");
                return data;
            }
            //client
            var client = await this._clientRepo.GetFirstAsync(x =>
            x.UID == client_id &&
            x.ClientSecretUID == client_secret &&
            x.IsRemove <= 0);
            if (client == null)
            {
                data.SetErrorMsg("应用不存在");
                return data;
            }
            //scope
            var scope_names = code.ScopesJson.JsonToEntity<List<string>>(throwIfException: false);
            if (!ValidateHelper.IsPlumpList(scope_names))
            {
                data.SetErrorMsg("scope为空");
                return data;
            }
            var scopes = await this._scopeRepo.GetListAsync(x => scope_names.Contains(x.Name) && x.IsRemove <= 0);
            if (scopes.Count != scope_names.Count)
            {
                data.SetErrorMsg("scope数据存在错误");
                return data;
            }

            //create new token
            var token = new TokenBase()
            {
                ExpiryTime = now.AddDays(TokenConfig.TokenExpireDays),
                RefreshToken = Com.GetUUID(),
                ScopesInfoJson = scopes.Select(x => new { uid = x.UID, name = x.Name }).ToJson(),
                ClientUID = code.ClientUID,
                UserUID = code.UserUID
            };
            token.Init("token");
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
            //scope map
            var scope_list = scopes.Select(x =>
            {
                var s = new TokenScopeBase()
                {
                    ScopeUID = x.UID,
                    TokenUID = token.UID,
                };
                s.Init("token-scope");
                return s;
            }).ToArray();
            if (await this._tokenScopeRepo.AddAsync(scope_list) <= 0)
            {
                data.SetErrorMsg("保存scope失败");
                return data;
            }

            //clear old token
            await ClearOldToken(code.UserUID);

            data.SetSuccessData(token);
            return data;
        }
        private async Task<bool> RefreshToken(TokenBase tk)
        {
            var success = false;
            await this._tokenRepo.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;
                var token_query = db.Set<TokenBase>();
                var token = await token_query.Where(x => x.UID == tk.UID && x.ExpiryTime > now && x.IsRemove <= 0).FirstOrDefaultAsync();

                if (token != null)
                {
                    //refresh expire time
                    token.ExpiryTime = now.AddDays(TokenConfig.TokenExpireDays);
                    token.UpdateTime = now;
                    token.RefreshTime = now;

                    success = await db.SaveChangesAsync() > 0;
                }

                return true;
            });
            return success;
        }
        private async Task<TokenBase> FindTokenAsync(string client_uid, string token_uid)
        {
            var token = default(TokenBase);
            await this._tokenRepo.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;
                var token_query = db.Set<TokenBase>().AsNoTrackingQueryable();
                token = await token_query.Where(x =>
                x.UID == token_uid &&
                x.ClientUID == client_uid &&
                x.ExpiryTime > now &&
                x.IsRemove <= 0).FirstOrDefaultAsync();

                if (token != null)
                {
                    //对于scope不去排除isremove，这个条件在授权的时候已经拦截了
                    var scope_uids = db.Set<TokenScopeBase>().AsNoTrackingQueryable().Where(x => x.TokenUID == token.UID).Select(x => x.ScopeUID);
                    var scope_query = db.Set<ScopeBase>().AsNoTrackingQueryable();
                    token.Scopes = (await scope_query.Where(x => scope_uids.Contains(x.UID)).ToListAsync()).Select(x => (AuthScopeBase)x).ToList();

                    //自动刷新过期时间
                    if ((token.ExpiryTime - now).TotalDays < (TokenConfig.TokenExpireDays / 2.0))
                    {
                        if (!await this.RefreshToken(token))
                        {
                            $"自动刷新token失败:{token.ToJson()}".AddBusinessInfoLog();
                        }
                    }
                }
                return true;
            });
            return token;
        }
        #endregion
    }
}
