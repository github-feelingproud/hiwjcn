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

        public virtual async Task<(AuthCode code, string msg)> CreateCode(string client_uid, List<string> scopes, string user_uid)
        {
            if (!ValidateHelper.IsPlumpList(scopes)) { return (null, "scopes为空"); }
            var code = new AuthCode()
            {
                UID = Com.GetUUID(),
                UserUID = user_uid,
                ClientUID = client_uid,
                ScopesJson = scopes.ToJson(),
                CreateTime = DateTime.Now
            };

            var border = DateTime.Now.GetDateBorder();
            if (await this._AuthCodeRepository.GetCountAsync(x => x.ClientUID == client_uid && x.UserUID == user_uid && x.CreateTime >= border.start && x.CreateTime < border.end) >= 2000)
            {
                return (null, "授权过多");
            }

            if (await this._AuthCodeRepository.AddAsync(code) > 0)
            {
                return (code, SUCCESS);
            }
            return (null, "保存票据失败");
        }

        public virtual async Task<(AuthToken token, string msg)> CreateToken(string client_id, string client_secret, string code_uid)
        {
            var now = DateTime.Now;
            var expire = now.AddMinutes(-5);
            var code = await this._AuthCodeRepository.GetFirstAsync(x => x.UID == code_uid && x.ClientUID == client_id && x.CreateTime > expire);
            if (code == null) { return (null, "code已失效"); }
            var scope_names = code.ScopesJson.JsonToEntity<List<string>>(false);
            if (!ValidateHelper.IsPlumpList(scope_names))
            {
                return (null, "scope为空");
            }

            var client = await this._AuthClientRepository.GetFirstAsync(x => x.UID == client_id && x.ClientSecretUID == client_secret);
            if (client == null) { return (null, "应用不存在"); }

            AuthToken the_choosen_token = null;

            var old_token_list = await this._AuthTokenRepository.GetListAsync(x => x.ClientUID == client_id && x.UserUID == code.UserUID);
            if (old_token_list.Count > 0)
            {
                var reuse_token = old_token_list.OrderByDescending(x => x.CreateTime).FirstOrDefault();
                reuse_token.ExpiryTime = now.AddDays(ExpireDays);
                reuse_token.RefreshTime = now;
                if (await this._AuthTokenRepository.UpdateAsync(reuse_token) <= 0)
                {
                    return (null, "刷新token失败");
                }

                the_choosen_token = reuse_token;

                {
                    //clear old token
                    var to_clear = old_token_list.Where(x => x.UID != reuse_token.UID).ToList();
                    if (ValidateHelper.IsPlumpList(to_clear))
                    {
                        if (await this._AuthTokenRepository.DeleteAsync(to_clear.ToArray()) <= 0)
                        {
                            $"清除旧token失败，数据：{to_clear.ToJson()}".AddBusinessInfoLog();
                        }
                    }
                }
            }
            else
            {
                //create new token
                var token = new AuthToken()
                {
                    UID = Com.GetUUID(),
                    CreateTime = now,
                    ExpiryTime = now.AddDays(ExpireDays),
                    RefreshToken = Com.GetUUID()
                };
                if (!token.IsValid(out var msg))
                {
                    return (null, msg);
                }
                if (await this._AuthTokenRepository.AddAsync(token) <= 0)
                {
                    return (null, "保存token失败");
                }
                the_choosen_token = token;
            }

            //删除旧的scope，添加新的scope
            var scopes = await this._AuthScopeRepository.GetListAsync(x => scope_names.Contains(x.Name));
            if (scopes.Count != scope_names.Count)
            {
                return (null, "scope数据存在错误");
            }
            var old_scope = await this._AuthTokenScopeRepository.GetListAsync(x => x.TokenUID == the_choosen_token.UID);
            if (ValidateHelper.IsPlumpList(old_scope) && await this._AuthTokenScopeRepository.DeleteAsync(old_scope.ToArray()) <= 0)
            {
                return (null, "删除旧scope失败");
            }
            var scope_list = scopes.Select(x => new AuthTokenScope()
            {
                UID = Com.GetUUID(),
                ScopeUID = x.UID,
                TokenUID = the_choosen_token.UID,
                CreateTime = now
            }).ToArray();
            if (ValidateHelper.IsPlumpList(scope_list) && await this._AuthTokenScopeRepository.AddAsync(scope_list) <= 0)
            {
                throw new MsgException("保存scope失败");
            }

            return (the_choosen_token, SUCCESS);
        }

        public async Task<string> DeleteToken(string client_uid, string user_uid)
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

        /// <summary>
        /// 直接获取client
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public async Task<PagerData<AuthClient>> GetMyAuthorizedClients_(string user_id, int page, int pagesize)
        {
            var data = new PagerData<AuthClient>();

            await this._AuthClientRepository.PrepareSessionAsync(async db =>
            {
                var client_query = db.Set<AuthClient>().AsNoTrackingQueryable();
                var token_query = db.Set<AuthToken>().AsNoTrackingQueryable();

                var client_uids = token_query.Where(x => x.UserUID == user_id).Select(x => x.ClientUID);

                client_query = client_query.Where(x => client_uids.Contains(x.UID));

                data.ItemCount = await client_query.CountAsync();
                client_query = client_query.OrderBy(x => x.ClientName).QueryPage(page, pagesize);
                data.DataList = await client_query.ToListAsync();

                if (ValidateHelper.IsPlumpList(data.DataList))
                {
                    //
                }

                return await Task.FromResult(true);
            });
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据token获取client
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
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
