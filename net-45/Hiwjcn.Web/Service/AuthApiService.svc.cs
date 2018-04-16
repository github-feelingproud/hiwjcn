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
using Lib.mvc.auth.api;

namespace Hiwjcn.Web.Service
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“AuthApiService”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 AuthApiService.svc 或 AuthApiService.svc.cs，然后开始调试。
    /// <summary>
    /// 请不要改名字或者移动位置，代码中有路径引用
    /// </summary>
    public class AuthApiService : IAuthApiWcfServiceContract
    {
        private async Task<T> X<T>(Func<IAuthApi, Task<T>> func)
        {
            using (var s = AutofacIocContext.Instance.Scope())
            {
                return await func.Invoke(s.Resolve_<IAuthApi>());
            }
        }

        public async Task<_<TokenModel>> GetAccessToken(string user_uid)
        {
            return await this.X(async s => await s.CreateAccessTokenAsync(user_uid));
        }

        public async Task<_<LoginUserInfo>> ValidUserByOneTimeCode(string phone, string sms)
        {
            return await this.X(async s => await s.ValidUserByOneTimeCodeAsync(phone, sms));
        }

        public async Task<_<LoginUserInfo>> ValidUserByPassword(string username, string password)
        {
            return await this.X(async s => await s.ValidUserByPasswordAsync(username, password));
        }

        public async Task<_<LoginUserInfo>> GetLoginUserInfoByToken(string access_token)
        {
            return await this.X(async s => await s.GetLoginUserInfoByTokenAsync(access_token));
        }

        public async Task<_<string>> RemoveCache(CacheBundle data)
        {
            return await this.X(async s => await s.RemoveCacheAsync(data));
        }
    }
}
