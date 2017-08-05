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
        public Task<ResultMsg<AuthCodeWcf>> CreateCode(string client_uid, List<string> scopes, string user_uid)
        {
            throw new NotImplementedException();
        }

        public Task<ResultMsg<AuthTokenWcf>> CreateToken(string client_uid, string client_secret, string code_uid)
        {
            throw new NotImplementedException();
        }

        public Task<ResultMsg<AuthTokenWcf>> FindToken(string token_uid)
        {
            throw new NotImplementedException();
        }

        public Task<PagerData<AuthClientWcf>> GetMyAuthorizedClients(string user_id, string q, int page, int pagesize)
        {
            throw new NotImplementedException();
        }

        public Task<List<AuthScopeWcf>> GetScopesOrDefault(string[] scopes)
        {
            throw new NotImplementedException();
        }
    }
}
