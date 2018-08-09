using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.auth
{
    /// <summary>
    /// 贯穿整个请求的登录用户上下文
    /// </summary>
    public interface IScopedUserContext : IDisposable
    {
        LoginUserInfo User { get; }

        bool IsLogin { get; }

        bool HasPermission(string permission);
    }
}
