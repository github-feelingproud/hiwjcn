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
        public static readonly int TokenExpireDays = (ConfigurationManager.AppSettings["AuthExpireDays"] ?? "30").ToInt(30);
        public static readonly int CodeExpireMinutes = (ConfigurationManager.AppSettings["CodeExpireMinutes"] ?? "5").ToInt(5);
        public static readonly int MaxCodeCreatedDaily = (ConfigurationManager.AppSettings["MaxCodeCreatedDaily"] ?? "2000").ToInt(2000);

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
            if (model == null)
            {
                return "对象为空";
            }
            return base.CheckModel(model);
        }

        private async Task ClearOldCode(string user_uid)
        {
            var expire = DateTime.Now.AddMinutes(-CodeExpireMinutes);
            await this._AuthCodeRepository.DeleteWhereAsync(x => x.UserUID == user_uid && x.CreateTime < expire);
        }

        public virtual async Task<(AuthCode code, string msg)> CreateCodeAsync(
            string client_uid, List<string> scopes, string user_uid)
        {
            if (!ValidateHelper.IsPlumpList(scopes)) { return (null, "scopes为空"); }
            var now = DateTime.Now;

            var code = new AuthCode()
            {
                UID = Com.GetUUID(),
                UserUID = user_uid,
                ClientUID = client_uid,
                ScopesJson = scopes.ToJson(),
                CreateTime = now
            };

            var border = now.GetDateBorder();
            if (await this._AuthCodeRepository.GetCountAsync(x => x.ClientUID == client_uid && x.UserUID == user_uid && x.CreateTime >= border.start && x.CreateTime < border.end) >= MaxCodeCreatedDaily)
            {
                return (null, "授权过多");
            }

            var ok = await this._AuthCodeRepository.AddAsync(code) > 0;

            if (ok)
            {
                //clear old data
                await this.ClearOldCode(user_uid);

                return (code, SUCCESS);
            }
            throw new MsgException("保存票据失败");
        }

        private async Task ClearOldToken(string user_uid)
        {
            var now = DateTime.Now;
            await this._AuthTokenRepository.PrepareSessionAsync(async db =>
            {
                var set = db.Set<AuthToken>();
                var old_tokens = set.Where(x => x.UserUID == user_uid && x.ExpiryTime < now);
                set.RemoveRange(old_tokens);

                var map = db.Set<AuthTokenScope>();
                map.RemoveRange(map.Where(x => old_tokens.Select(m => m.UID).Contains(x.TokenUID)));

                await db.SaveChangesAsync();
                return true;
            });
        }

        public virtual async Task<(AuthToken token, string msg)> CreateTokenAsync(
            string client_id, string client_secret, string code_uid)
        {
            try
            {
                var now = DateTime.Now;

                //code
                var expire = now.AddMinutes(-CodeExpireMinutes);
                var code = await this._AuthCodeRepository.GetFirstAsync(x => x.UID == code_uid && x.ClientUID == client_id && x.CreateTime > expire);
                if (code == null)
                {
                    throw new MsgException("code已失效");
                }
                //client
                var client = await this._AuthClientRepository.GetFirstAsync(x => x.UID == client_id && x.ClientSecretUID == client_secret && x.IsRemove <= 0);
                if (client == null)
                {
                    throw new MsgException("应用不存在");
                }
                //scope
                var scope_names = code.ScopesJson.JsonToEntity<List<string>>(throwIfException: false);
                if (!ValidateHelper.IsPlumpList(scope_names))
                {
                    throw new MsgException("scope为空");
                }
                var scopes = await this._AuthScopeRepository.GetListAsync(x => scope_names.Contains(x.Name));
                if (scopes.Count != scope_names.Count)
                {
                    throw new MsgException("scope数据存在错误");
                }

                //create new token
                var token = new AuthToken()
                {
                    UID = Com.GetUUID(),
                    CreateTime = now,
                    ExpiryTime = now.AddDays(TokenExpireDays),
                    RefreshToken = Com.GetUUID(),
                    ScopesInfoJson = scopes.Select(x => new { uid = x.UID, name = x.Name }).ToJson()
                };
                if (!token.IsValid(out var msg))
                {
                    throw new MsgException(msg);
                }
                if (await this._AuthTokenRepository.AddAsync(token) <= 0)
                {
                    throw new MsgException("保存token失败");
                }
                //scope map
                var scope_list = scopes.Select(x => new AuthTokenScope()
                {
                    UID = Com.GetUUID(),
                    ScopeUID = x.UID,
                    TokenUID = token.UID,
                    CreateTime = now
                }).ToArray();
                if (await this._AuthTokenScopeRepository.AddAsync(scope_list) <= 0)
                {
                    throw new MsgException("保存scope失败");
                }

                {
                    //clear old token
                    await ClearOldToken(code.UserUID);
                }
                return (token, SUCCESS);
            }
            catch (MsgException e)
            {
                return (null, e.Message);
            }
        }

        public async Task<string> DeleteClientAsync(string client_uid, string user_uid)
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

        private async Task<bool> RefreshToken(AuthToken tk)
        {
            var success = false;
            await this._AuthTokenRepository.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;
                var token_query = db.Set<AuthToken>();
                var token = await token_query.Where(x => x.UID == tk.UID && x.ExpiryTime > now).FirstOrDefaultAsync();

                if (token != null)
                {                //refresh expire time
                    token.ExpiryTime = now.AddDays(TokenExpireDays);

                    success = await db.SaveChangesAsync() > 0;
                }

                return true;
            });
            return success;
        }

        public async Task<AuthToken> FindTokenAsync(string token_uid)
        {
            var token = default(AuthToken);
            await this._AuthTokenRepository.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;
                var token_query = db.Set<AuthToken>().AsNoTrackingQueryable();
                token = await token_query.Where(x => x.UID == token_uid && x.ExpiryTime > now).FirstOrDefaultAsync();

                if (token != null)
                {
                    var scope_uids = db.Set<AuthTokenScope>().AsNoTrackingQueryable().Where(x => x.TokenUID == token.UID).Select(x => x.ScopeUID);
                    var scope_query = db.Set<AuthScope>().AsNoTrackingQueryable();
                    token.Scopes = await scope_query.Where(x => scope_uids.Contains(x.UID)).ToListAsync();

                    //自动刷新过期时间
                    if ((token.ExpiryTime - now).TotalDays < (TokenExpireDays / 2.0))
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

        /// <summary>
        /// 直接获取client
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public async Task<PagerData<AuthClient>> GetMyAuthorizedClientsAsync(string user_id, string q, int page, int pagesize)
        {
            var data = new PagerData<AuthClient>();

            await this._AuthClientRepository.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;

                var client_query = db.Set<AuthClient>().AsNoTrackingQueryable();
                var token_query = db.Set<AuthToken>().AsNoTrackingQueryable();

                var client_uids = token_query.Where(x => x.UserUID == user_id && x.ExpiryTime < now).Select(x => x.ClientUID);

                client_query = client_query.Where(x => client_uids.Contains(x.UID) && x.IsRemove <= 0);
                if (ValidateHelper.IsPlumpString(q))
                {
                    client_query = client_query.Where(x => x.ClientName.Contains(q) || x.Description.Contains(q));
                }

                data.ItemCount = await client_query.CountAsync();
                client_query = client_query.OrderBy(x => x.ClientName).QueryPage(page, pagesize);
                data.DataList = await client_query.ToListAsync();

                if (ValidateHelper.IsPlumpList(data.DataList))
                {
                    //
                }

                return true;
            });

            return data;
        }
    }
}
