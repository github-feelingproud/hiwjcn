using Microsoft.AspNetCore.Http;

namespace Lib.mvc.auth.validation
{
    /// <summary>
    /// 获取token和client 信息的渠道
    /// </summary>
    public interface IAuthDataProvider
    {
        string GetToken(HttpContext context);
    }
}
