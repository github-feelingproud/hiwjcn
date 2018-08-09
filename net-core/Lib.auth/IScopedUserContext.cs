using System;
using System.Threading.Tasks;

namespace Lib.auth
{
    /// <summary>
    /// 贯穿整个请求的登录用户上下文
    /// </summary>
    public interface IScopedUserContext : IDisposable
    {
        Task<LoginUserInfo> GetLoginUserAsync();
    }
}
