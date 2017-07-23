using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Hiwjcn.Core.Domain.Auth;

namespace Hiwjcn.Web.wcf
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“AuthApi”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 AuthApi.svc 或 AuthApi.svc.cs，然后开始调试。
    public class AuthApi : IAuthApi
    {
        public async Task<AuthTokenWcf> CreateToken()
        {
            return await Task.FromResult(new AuthTokenWcf());
        }
    }
}
