using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;
using Lib.data;
using Lib.mvc;
using Lib.helper;
using Lib.extension;
using System.Data.Entity;
using Lib.core;

namespace Lib.infrastructure.service
{
    /// <summary>
    /// auth相关帮助类
    /// </summary>
    public interface IAuthServiceBase<ClientBase, ScopeBase, TokenBase, CodeBase, TokenScopeBase>
        where ClientBase : AuthClientBase, new()
        where ScopeBase : AuthScopeBase, new()
        where TokenBase : AuthTokenBase, new()
        where CodeBase : AuthCodeBase, new()
        where TokenScopeBase : AuthTokenScopeBase, new()
    {
        Task<PagerData<ClientBase>> QueryClientListAsync(
            string user_uid = null, string q = null, bool? is_active = null, bool? is_remove = null,
            int page = 1, int pagesize = 10);

        Task<_<CodeBase>> CreateCodeAsync(string client_uid, List<string> scopes, string user_uid);
        Task<TokenBase> FindTokenAsync(string client_uid, string token_uid);
        Task<_<TokenBase>> CreateTokenAsync(string client_id, string client_secret, string code_uid);
    }

    public abstract class AuthServiceBase<ClientBase, ScopeBase, TokenBase, CodeBase, TokenScopeBase> :
        IAuthServiceBase<ClientBase, ScopeBase, TokenBase, CodeBase, TokenScopeBase>
        where ClientBase : AuthClientBase, new()
        where ScopeBase : AuthScopeBase, new()
        where TokenBase : AuthTokenBase, new()
        where CodeBase : AuthCodeBase, new()
        where TokenScopeBase : AuthTokenScopeBase, new()
    {
        protected readonly IRepository<ClientBase> _clientRepo;
        protected readonly IRepository<ScopeBase> _scopeRepo;
        protected readonly IRepository<TokenBase> _tokenRepo;
        protected readonly IRepository<CodeBase> _codeRepo;
        protected readonly IRepository<TokenScopeBase> _tokenScopeRepo;

        public AuthServiceBase(
            IRepository<ClientBase> _clientRepo,
            IRepository<ScopeBase> _scopeRepo,
            IRepository<TokenBase> _tokenRepo,
            IRepository<CodeBase> _codeRepo,
            IRepository<TokenScopeBase> _tokenScopeRepo)
        {
            this._clientRepo = _clientRepo;
            this._scopeRepo = _scopeRepo;
            this._tokenRepo = _tokenRepo;
            this._codeRepo = _codeRepo;
            this._tokenScopeRepo = _tokenScopeRepo;
        }


        #region
        public virtual async Task<PagerData<ClientBase>> QueryClientListAsync(
            string user_uid = null, string q = null, bool? is_active = null, bool? is_remove = null,
            int page = 1, int pagesize = 10)
        {
            var data = new PagerData<ClientBase>();

            await this._clientRepo.PrepareSessionAsync(async db =>
            {
                var query = db.Set<ClientBase>().AsNoTrackingQueryable();
                if (is_remove != null)
                {
                    if (is_remove.Value)
                    {
                        query = query.Where(x => x.IsRemove > 0);
                    }
                    else
                    {
                        query = query.Where(x => x.IsRemove <= 0);
                    }
                }
                if (ValidateHelper.IsPlumpString(user_uid))
                {
                    query = query.Where(x => x.UserUID == user_uid);
                }
                if (is_active != null)
                {
                    if (is_active.Value)
                    {
                        query = query.Where(x => x.IsActive > 0);
                    }
                    else
                    {
                        query = query.Where(x => x.IsActive <= 0);
                    }
                }
                if (ValidateHelper.IsPlumpString(q))
                {
                    query = query.Where(x => x.ClientName.Contains(q));
                }

                data.ItemCount = await query.CountAsync();
                data.DataList = await query
                .OrderByDescending(x => x.IsOfficial).OrderByDescending(x => x.CreateTime)
                .QueryPage(page, pagesize).ToListAsync();
                return true;
            });

            return data;
        }
        #endregion

        private async Task ClearOldCode(string user_uid)
        {
            var expire = DateTime.Now.AddMinutes(-TokenConfig.CodeExpireMinutes);
            await this._codeRepo.DeleteWhereAsync(x => x.UserUID == user_uid && x.CreateTime < expire);
        }

        public virtual async Task<_<CodeBase>> CreateCodeAsync(string client_uid, List<string> scopes, string user_uid)
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

        public virtual async Task<_<TokenBase>> CreateTokenAsync(string client_id, string client_secret, string code_uid)
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

        public virtual async Task<TokenBase> FindTokenAsync(string client_uid, string token_uid)
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
    }
}
