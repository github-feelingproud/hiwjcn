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
        public AuthClientService(IRepository<AuthClient> _AuthClientRepository)
        {
            this._AuthClientRepository = _AuthClientRepository;
        }

        public async Task<string> AddClientAsync(AuthClient client)
        {
            if (!this.CheckModel(client, out var msg))
            {
                return msg;
            }
            var res = await this._AuthClientRepository.AddAsync(client);
            return res > 0 ? SUCCESS : "保存失败";
        }

        public override string CheckModel(AuthClient model)
        {
            return base.CheckModel(model);
        }

        public async Task<string> DeleteClientAsync(string client_uid, string user_uid)
        {
            var client = await this._AuthClientRepository.GetFirstAsync(x => x.UID == client_uid && x.UserUID == user_uid) ?? throw new SourceNotExistException();
            var res = await this._AuthClientRepository.DeleteAsync(client);
            return res > 0 ? SUCCESS : "删除失败";
        }

        public async Task<string> EnableOrDisableClientAsync(string client_uid, string user_uid)
        {
            var client = await this._AuthClientRepository.GetFirstAsync(x => x.UID == client_uid && x.UserUID == user_uid) ?? throw new SourceNotExistException();

            client.IsRemove = (!client.IsRemove.ToString().ToBool()).ToString().ToBoolInt();

            if (!this.CheckModel(client, out var msg))
            {
                return msg;
            }

            var res = await this._AuthClientRepository.UpdateAsync(client);
            return res > 0 ? SUCCESS : "保存失败";
        }

        public async Task<PagerData<AuthClient>> QueryListAsync(string user_uid, string q, int page, int pagesize)
        {
            var data = new PagerData<AuthClient>();

            await this._AuthClientRepository.PrepareSessionAsync(async db =>
            {
                var query = db.Set<AuthClient>().AsNoTrackingQueryable();
                query = query.Where(x => x.UserUID == user_uid);
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
            var client = await this._AuthClientRepository.GetFirstAsync(x => x.UID == updatemodel.UID && x.UserUID == updatemodel.UserUID) ?? throw new SourceNotExistException();

            client.ClientName = updatemodel.ClientName;
            client.ClientUrl = updatemodel.ClientUrl;
            client.LogoUrl = updatemodel.LogoUrl;
            client.UpdateTime = DateTime.Now;

            if (!this.CheckModel(client, out var msg))
            {
                return msg;
            }

            var res = await this._AuthClientRepository.UpdateAsync(client);
            return res > 0 ? SUCCESS : "更新失败";
        }
    }
}
