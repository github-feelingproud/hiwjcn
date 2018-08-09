using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.auth
{
    public interface ICacheKeyManager
    {
        string AuthTokenCacheKey(string token);

        string AuthUserInfoCacheKey(string user_uid);
    }
}
