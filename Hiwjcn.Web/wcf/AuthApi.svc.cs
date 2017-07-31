using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Hiwjcn.Core.Domain.Auth;
using Lib.ioc;
using Lib.helper;
using Lib.extension;
using Lib.core;
using Hiwjcn.Core.Infrastructure.Auth;
using Lib.mvc;

namespace Hiwjcn.Web.wcf
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“AuthApi”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 AuthApi.svc 或 AuthApi.svc.cs，然后开始调试。
    public class AuthApi : IAuthApi
    {
        public async Task<ResultMsg<AuthCodeWcf>> CreateCode(string client_uid, List<string> scopes, string user_uid)
        {
            var code = await AppContext.ScopeAsync(async s =>
              {
                  return await s.Resolve_<IAuthTokenService>().CreateCodeAsync(client_uid, scopes, user_uid);
              });

            return new ResultMsg<AuthCodeWcf>()
            {
                Success = !ValidateHelper.IsPlumpString(code.msg),
                ErrorMsg = code.msg,
                Data = code.code
            };
        }

        public async Task<ResultMsg<AuthTokenWcf>> CreateToken(string client_uid, string client_secret, string code_uid)
        {
            var token = await AppContext.ScopeAsync(async s =>
            {
                return await s.Resolve_<IAuthTokenService>().CreateTokenAsync(client_uid, client_secret, code_uid);
            });

            return new ResultMsg<AuthTokenWcf>()
            {
                Success = !ValidateHelper.IsPlumpString(token.msg),
                ErrorMsg = token.msg,
                Data = token.token
            };
        }

        public async Task<ResultMsg<AuthTokenWcf>> FindToken(string token_uid)
        {
            var token = await AppContext.ScopeAsync(async s =>
            {
                return await s.Resolve_<IAuthTokenService>().FindTokenAsync("", token_uid);
            });

            return new ResultMsg<AuthTokenWcf>()
            {
                Success = token != null,
                Data = token
            };
        }

        public async Task<PagerData<AuthClientWcf>> GetMyAuthorizedClients(string user_id, string q, int page, int pagesize)
        {
            var clients = await AppContext.ScopeAsync(async s =>
            {
                return await s.Resolve_<IAuthTokenService>().GetMyAuthorizedClientsAsync(user_id, q, page, pagesize);
            });

            return new PagerData<AuthClientWcf>()
            {
                ItemCount = clients.ItemCount,
                DataList = clients.DataList.Select(x => (AuthClientWcf)x).ToList()
            };
        }

        public async Task<List<AuthScopeWcf>> GetScopesOrDefault(string[] scopes)
        {
            var list = await AppContext.ScopeAsync(async s =>
            {
                return await s.Resolve_<IAuthScopeService>().GetScopesOrDefault(scopes);
            });

            return list.Select(x => (AuthScopeWcf)x).ToList();
        }
    }
}
