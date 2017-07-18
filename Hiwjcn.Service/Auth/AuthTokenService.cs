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
using Model.User;

namespace Hiwjcn.Bll.Auth
{
    public class AuthTokenService : ServiceBase<AuthToken>, IAuthTokenService
    {
        private readonly IRepository<AuthToken> _AuthTokenRepository;
        private readonly IRepository<AuthTokenScope> _AuthTokenScopeRepository;
        private readonly IRepository<AuthScope> _AuthScopeRepository;
        private readonly IRepository<AuthCode> _AuthCodeRepository;
        private readonly IRepository<AuthClient> _AuthClientRepository;
        public AuthTokenService(
            IRepository<AuthToken> _AuthTokenRepository,
            IRepository<AuthTokenScope> _AuthTokenScopeRepository,
            IRepository<AuthScope> _AuthScopeRepository,
            IRepository<AuthCode> _AuthCodeRepository,
            IRepository<AuthClient> _AuthClientRepository)
        {
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

        public virtual async Task<string> CreateToken(AuthToken token)
        {
            if (!this.CheckModel(token, out var msg))
            {
                return msg;
            }
            if (!ValidateHelper.IsPlumpList(token.ScopeNames))
            {
                token.ScopeNames = (await this._AuthScopeRepository.GetListAsync(x => x.IsDefault > 0)).Select(x => x.Name).ToList();
                if (!ValidateHelper.IsPlumpList(token.ScopeNames))
                {
                    throw new MsgException("没有设置默认scope");
                }
            }
            else
            {
                var count = await this._AuthScopeRepository.GetCountAsync(x => token.ScopeNames.Contains(x.Name));
                if (count != token.ScopeNames.Count)
                {
                    throw new MsgException("提交数据错误");
                }
            }
            if ((await this._AuthTokenRepository.AddAsync(token)) <= 0)
            {
                throw new MsgException("保存token失败");
            }
            var scopes = token.ScopeNames.Select(x => new AuthTokenScope()
            {
                UID = Com.GetUUID(),
                CreateTime = DateTime.Now
            }).ToArray();
            if ((await this._AuthTokenScopeRepository.AddAsync(scopes)) <= 0)
            {
                throw new MsgException("保存token scope map失败");
            }
            return SUCCESS;
        }

        public Task<AuthToken> FindToken(string token_uid)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<AuthToken> RefreshToken(string refresh_token_uid)
        {
            var token = await this._AuthTokenRepository.GetFirstAsync(x => x.RefreshToken == refresh_token_uid && x.ExpiryTime < DateTime.Now) ?? throw new SourceNotExistException();
            token.ExpiryTime = DateTime.Now.AddDays(30);
            if (!this.CheckModel(token, out var msg))
            {
                throw new MsgException(msg);
            }
            if ((await this._AuthTokenRepository.UpdateAsync(token)) <= 0)
            {
                throw new MsgException("更新token失败");
            }
            return token;
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
                    //owner and client
                    var client = db.Set<AuthClient>().AsNoTrackingQueryable();
                    var client_uids = data.DataList.Select(x => x.ClientUID);
                    var all_clients = await client.Where(x => client_uids.Contains(x.UID)).ToListAsync();
                    var all_client_owner_uids = all_clients.Select(x => x.UserUID);
                    var user = db.Set<UserModel>().AsNoTrackingQueryable();
                    var all_client_owners = await user.Where(x => all_client_owner_uids.Contains(x.UID)).ToListAsync();
                    foreach (var c in all_clients)
                    {
                        c.Owner = all_client_owners.Where(x => x.UID == c.UserUID).FirstOrDefault();
                    }
                    foreach (var m in data.DataList)
                    {
                        m.Client = all_clients.Where(x => x.UID == m.ClientUID).FirstOrDefault();
                    }
                    //scopes
                    var scope = db.Set<AuthScope>().AsNoTrackingQueryable();
                    var scope_map = db.Set<AuthTokenScope>().AsNoTrackingQueryable();
                    var all_scope_map = await scope_map.Where(x => client_uids.Contains(x.TokenUID)).ToListAsync();
                    var all_scope_uids = all_scope_map.Select(x => x.ScopeUID);
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
