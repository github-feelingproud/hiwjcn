using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc;
using Lib.mvc.auth;
using Lib.mvc.user;
using Lib.ioc;

namespace Hiwjcn.Web.Service
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“AuthApiService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 AuthApiService.svc 或 AuthApiService.svc.cs，然后开始调试。
    public class AuthApiService : IAuthApiService
    {
        private async Task<T> X<T>(Func<IAuthApi, Task<T>> func) =>
            await AppContext.ScopeAsync(async s => await func.Invoke(s.Resolve_<IAuthApi>()));

        public async Task<_<TokenModel>> GetAccessToken(string client_id, string client_secret, string code, string grant_type)
        {
            return await this.X(async s => await s.GetAccessTokenAsync(client_id, client_secret, code, grant_type));
        }

        public async Task<_<string>> GetAuthCodeByOneTimeCode(string client_id, string scopeJson, string phone, string sms)
        {
            return await this.X(async s => await s.GetAuthCodeByOneTimeCodeAsync(client_id, scopeJson, phone, sms));
        }

        public async Task<_<string>> GetAuthCodeByPassword(string client_id, string scopeJson, string username, string password)
        {
            return await this.X(async s => await s.GetAuthCodeByPasswordAsync(client_id, scopeJson, username, password));
        }

        public async Task<_<LoginUserInfo>> GetLoginUserInfoByToken(string client_id, string access_token)
        {
            return await this.X(async s => await s.GetLoginUserInfoByTokenAsync(client_id, access_token));
        }
    }
}
