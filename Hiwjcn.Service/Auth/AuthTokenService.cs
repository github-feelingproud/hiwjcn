using Hiwjcn.Core.Infrastructure.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using Lib.infrastructure;
using Hiwjcn.Core.Domain.Auth;
using Lib.core;
using Lib.extension;
using Lib.helper;
using System.Data.Entity;
using Lib.events;
using System.Configuration;

namespace Hiwjcn.Bll.Auth
{
    public class AuthTokenService : ServiceBase<AuthToken>, IAuthTokenService
    {
        public static readonly int ExpireDays = (ConfigurationManager.AppSettings["AuthExpireDays"] ?? "30").ToInt(30);

        private readonly IRepository<AuthToken> _AuthTokenRepository;
        private readonly IRepository<AuthTokenScope> _AuthTokenScopeRepository;
        private readonly IRepository<AuthScope> _AuthScopeRepository;
        private readonly IRepository<AuthCode> _AuthCodeRepository;
        private readonly IRepository<AuthClient> _AuthClientRepository;

        private readonly IEventPublisher _publisher;

        public AuthTokenService(
            IEventPublisher _publisher,
            IRepository<AuthToken> _AuthTokenRepository,
            IRepository<AuthTokenScope> _AuthTokenScopeRepository,
            IRepository<AuthScope> _AuthScopeRepository,
            IRepository<AuthCode> _AuthCodeRepository,
            IRepository<AuthClient> _AuthClientRepository)
        {
            this._publisher = _publisher;

            this._AuthTokenRepository = _AuthTokenRepository;
            this._AuthTokenScopeRepository = _AuthTokenScopeRepository;
            this._AuthScopeRepository = _AuthScopeRepository;
            this._AuthCodeRepository = _AuthCodeRepository;
            this._AuthClientRepository = _AuthClientRepository;
        }

        public override string CheckModel(AuthToken model)
        {
            return base.CheckModel(model);
        }

        public virtual async Task<string> CreateToken(string client_uid, string user_uid)
        {
            var now = DateTime.Now;
            var token = new AuthToken()
            {
                UID = Com.GetUUID(),
                CreateTime = now,
                ExpiryTime = now.AddDays(ExpireDays),
                RefreshToken = Com.GetUUID()
            };

            if (!this.CheckModel(token, out var msg))
            {
                return msg;
            }
            if (!ValidateHelper.IsPlumpList(token.ScopeNames))
            {
                token.ScopeNames = (await this._AuthScopeRepository.GetListAsync(x => x.IsDefault > 0)).Select(x => x.Name).ToList();
                if (!ValidateHelper.IsPlumpList(token.ScopeNames))
                {
                    return "没有设置默认的scope";
                }
            }

            token.Scopes = await this._AuthScopeRepository.GetListAsync(x => token.ScopeNames.Contains(x.Name));
            if (token.Scopes.Count != token.ScopeNames.Count)
            {
                return "提交数据错误";
            }

            if ((await this._AuthTokenRepository.AddAsync(token)) <= 0)
            {
                return "保存token失败";
            }
            var scopes = token.Scopes.Select(x => new AuthTokenScope()
            {
                UID = Com.GetUUID(),

                TokenUID = token.UID,
                ScopeUID = x.UID,

                CreateTime = now
            }).ToArray();
            if ((await this._AuthTokenScopeRepository.AddAsync(scopes)) <= 0)
            {
                return "保存token scope map失败";
            }

            await this.RefreshToken(token);

            this._publisher.EntityInserted(token);

            return SUCCESS;
        }

        private async Task<string> DeleteToken(string client_uid, string user_uid)
        {
            var msg = SUCCESS;
            await this._AuthTokenRepository.PrepareSessionAsync(async db =>
            {
                var token_query = db.Set<AuthToken>();

                var token_to_delete = token_query.Where(x => x.ClientUID == client_uid && x.UserUID == user_uid);
                token_query.RemoveRange(token_to_delete);

                var scope_map_query = db.Set<AuthTokenScope>();
                scope_map_query.RemoveRange(scope_map_query.Where(x => token_to_delete.Select(m => m.UID).Contains(x.TokenUID)));

                if (await db.SaveChangesAsync() <= 0)
                {
                    msg = "删除token失败";
                }
                return true;
            });
            return msg;
        }

        private async Task RefreshToken(AuthToken tk)
        {
            await this._AuthTokenRepository.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;
                var token_query = db.Set<AuthToken>();
                var token = await token_query.Where(x => x.UID == tk.UID && x.ExpiryTime > now).FirstOrDefaultAsync();
                if (token == null) { return false; }

                //refresh expire time
                token.ExpiryTime = now.AddDays(ExpireDays);

                var token_to_delete = token_query.Where(x => x.ClientUID == token.ClientUID && x.UID != token.UID);

                //delete other token
                token_query.RemoveRange(token_to_delete);

                //delete other token scope map
                var scope_query = db.Set<AuthTokenScope>();
                scope_query.RemoveRange(scope_query.Where(x => token_to_delete.Select(m => m.UID).Contains(x.TokenUID)));

                //save changes
                if (await db.SaveChangesAsync() <= 0)
                {
                    $"自动刷新token失败:{token.ToJson()}".AddBusinessInfoLog();
                }

                this._publisher.EntityUpdated(token);

                return true;
            });
        }

        public async Task<AuthToken> FindToken(string token_uid)
        {
            var token = default(AuthToken);
            await this._AuthTokenRepository.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;
                var token_query = db.Set<AuthToken>();
                token = await token_query.Where(x => x.UID == token_uid && x.ExpiryTime > now).FirstOrDefaultAsync();

                if (token != null)
                {
                    var scope_uids = db.Set<AuthTokenScope>().AsNoTrackingQueryable().Where(x => x.TokenUID == token.UID).Select(x => x.ScopeUID);
                    var scope_query = db.Set<AuthScope>().AsNoTrackingQueryable();
                    token.Scopes = await scope_query.Where(x => scope_uids.Contains(x.UID)).ToListAsync();

                    //自动刷新过期时间
                    if ((token.ExpiryTime - now).TotalDays < (ExpireDays / 2.0))
                    {
                        await this.RefreshToken(token);
                    }
                }
                return true;
            });
            return token;
        }

        public virtual Task<AuthToken> RefreshToken(string refresh_token_uid)
        {
            throw new NotImplementedException();
        }

        public async Task<PagerData<AuthToken>> GetMyAuthorizedClients(string user_id, int page, int pagesize)
        {
            var data = new PagerData<AuthToken>();

            await this._AuthClientRepository.PrepareSessionAsync(async db =>
            {
                var token = db.Set<AuthToken>().AsNoTrackingQueryable();

                token = token.Where(x => x.UserUID == user_id);

                data.ItemCount = await token.CountAsync();
                token = token.OrderByDescending(x => x.RefreshTime).OrderByDescending(x => x.CreateTime).QueryPage(page, pagesize);
                data.DataList = await token.ToListAsync();

                if (ValidateHelper.IsPlumpList(data.DataList))
                {
                    var client_uids = data.DataList.Select(x => x.ClientUID);
                    //client
                    var client = db.Set<AuthClient>().AsNoTrackingQueryable();
                    var all_clients = await client.Where(x => client_uids.Contains(x.UID)).ToListAsync();
                    foreach (var m in data.DataList)
                    {
                        m.Client = all_clients.Where(x => x.UID == m.ClientUID).FirstOrDefault();
                    }
                    //scope map
                    var scope_map = db.Set<AuthTokenScope>().AsNoTrackingQueryable();
                    var all_scope_map = await scope_map.Where(x => client_uids.Contains(x.TokenUID)).ToListAsync();
                    var all_scope_uids = all_scope_map.Select(x => x.ScopeUID);
                    //scope
                    var scope = db.Set<AuthScope>().AsNoTrackingQueryable();
                    var all_scopes = await scope.Where(x => all_scope_uids.Contains(x.UID)).ToListAsync();

                    foreach (var m in data.DataList)
                    {
                        m.ScopeUIDS = all_scope_map.Where(x => x.TokenUID == m.UID).Select(x => x.ScopeUID).ToList();
                        m.Scopes = all_scopes.Where(x => m.ScopeUIDS.Contains(x.UID)).ToList();
                    }
                }

                return true;
            });

            return data;
        }
    }
}
