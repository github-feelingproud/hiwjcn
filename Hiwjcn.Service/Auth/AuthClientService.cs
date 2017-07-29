using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure;
using Hiwjcn.Core.Infrastructure.Auth;
using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Core.Data.Auth;
using Lib.data;
using Lib.core;
using Lib.extension;
using Lib.helper;
using System.Data.Entity;

namespace Hiwjcn.Bll.Auth
{
    public class AuthClientService : ServiceBase<AuthClient>, IAuthClientService
    {
        private readonly IRepository<AuthClient> _AuthClientRepository;
        private readonly IRepository<AuthClientCheckLog> _AuthClientCheckLogRepository;
        public AuthClientService(
            IRepository<AuthClient> _AuthClientRepository,
            IRepository<AuthClientCheckLog> _AuthClientCheckLogRepository)
        {
            this._AuthClientRepository = _AuthClientRepository;
            this._AuthClientCheckLogRepository = _AuthClientCheckLogRepository;
        }

        public async Task<string> AddClientAsync(AuthClient client)
        {
            client.Init("client");
            if (!this.CheckModel(client, out var msg))
            {
                return msg;
            }
            var ok = await this._AuthClientRepository.AddAsync(client) > 0;
            if (ok)
            {
                return SUCCESS;
            }
            throw new Exception("保存client异常");
        }

        public override string CheckModel(AuthClient model)
        {
            return base.CheckModel(model);
        }

        public async Task<string> DeleteClientAsync(string client_uid, string user_uid)
        {
            var client = await this._AuthClientRepository.GetFirstAsync(x => x.UID == client_uid && x.UserUID == user_uid);
            Com.Assert(client, x => x != null, $"找不到client[client_uid={client_uid},user_uid={user_uid}]");
            var ok = await this._AuthClientRepository.DeleteAsync(client) > 0;

            if (ok)
            {
                await this._AuthClientRepository.PrepareSessionAsync(async db =>
                {
                    var token_set = db.Set<AuthToken>();
                    var code_set = db.Set<AuthCode>();
                    var scope_set = db.Set<AuthTokenScope>();

                    var token_to_delete = token_set.Where(x => x.ClientUID == client_uid);
                    token_set.RemoveRange(token_to_delete);

                    var code_to_delete = code_set.Where(x => x.ClientUID == client_uid);
                    code_set.RemoveRange(code_to_delete);

                    var scope_to_delete = scope_set.Where(x => token_to_delete.Select(m => m.UID).Contains(x.TokenUID));
                    scope_set.RemoveRange(scope_to_delete);

                    await db.SaveChangesAsync();
                    return true;
                });
                return SUCCESS;
            }
            throw new Exception("删除client异常");
        }

        public async Task<string> EnableOrDisableClientAsync(string client_uid, string user_uid)
        {
            var client = await this._AuthClientRepository.GetFirstAsync(x => x.UID == client_uid && x.UserUID == user_uid);
            client = client ?? throw new Exception($"找不到client[client_uid={client_uid},user_uid={user_uid}]");
            client.IsRemove = (!client.IsRemove.ToString().ToBool()).ToString().ToBoolInt();
            client.UpdateTime = DateTime.Now;

            if (!this.CheckModel(client, out var msg))
            {
                return msg;
            }

            var ok = await this._AuthClientRepository.UpdateAsync(client) > 0;
            if (ok)
            {
                return SUCCESS;
            }
            throw new MsgException("更新client激活状态异常");
        }

        public async Task<AuthClient> GetByID(string uid)
        {
            var model = await this._AuthClientRepository.GetFirstAsync(x => x.UID == uid && x.IsActive > 0 && x.IsRemove <= 0);

            return model;
        }

        public async Task<string> ApproveClient(string client_uid, bool active, string reason)
        {
            var model = await this._AuthClientRepository.GetFirstAsync(x => x.UID == client_uid);
            Com.AssertNotNull(model, $"can not find client by uid:{client_uid}");
            var active_data = active.ToString().ToBoolInt();
            if (model.IsActive == active_data)
            {
                return "状态无需改变";
            }
            model.IsActive = active_data;
            if (await this._AuthClientRepository.UpdateAsync(model) > 0)
            {
                var log = new AuthClientCheckLog();
                log.Init("clientchecklog");
                log.CheckStatus = model.IsActive;
                log.Msg = reason ?? "no reason";
                await this._AuthClientCheckLogRepository.AddAsync(log);

                return SUCCESS;
            }
            throw new Exception("更新client失败");
        }

        public async Task<PagerData<AuthClient>> QueryListAsync(
            string user_uid = null, string q = null, bool? is_active = null,
            int page = 1, int pagesize = 10)
        {
            var data = new PagerData<AuthClient>();

            await this._AuthClientRepository.PrepareSessionAsync(async db =>
            {
                var query = db.Set<AuthClient>().AsNoTrackingQueryable();
                query = query.Where(x => x.IsRemove <= 0);
                query = query.Where(x => x.UserUID == user_uid);
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
                data.DataList = await query.OrderByDescending(x => x.CreateTime).QueryPage(page, pagesize).ToListAsync();
                return true;
            });

            return data;
        }

        public async Task<string> UpdateClientAsync(AuthClient updatemodel)
        {
            var client = await this._AuthClientRepository.GetFirstAsync(x => x.UID == updatemodel.UID && x.UserUID == updatemodel.UserUID);
            Com.Assert(client, x => x != null, $"client不存在:{updatemodel.ToJson()}");

            client.ClientName = updatemodel.ClientName;
            client.ClientUrl = updatemodel.ClientUrl;
            client.LogoUrl = updatemodel.LogoUrl;
            client.UpdateTime = DateTime.Now;

            if (!this.CheckModel(client, out var msg))
            {
                return msg;
            }

            var ok = await this._AuthClientRepository.UpdateAsync(client) > 0;
            if (ok)
            {
                return SUCCESS;
            }
            throw new MsgException("更新client异常");
        }
    }
}
