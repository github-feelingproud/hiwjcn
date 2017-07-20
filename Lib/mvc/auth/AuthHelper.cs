using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.ioc;

namespace Lib.mvc.auth
{
    public enum AuthMode : int
    {
        Server = 1,
        Client = 2
    }

    public static class AuthHelper
    {
        public static void CheckDependency()
        {
            if (!AppContext.IsRegistered<IAuthStoreProvider>())
            {
                throw new Exception("请注册...");
            }
            if (!AppContext.IsRegistered<IAuthTokenCheckProvider>())
            {
                throw new Exception("请注册...");
            }

            if (!AppContext.IsRegistered<AuthServerConfig>())
            {
                throw new Exception("请注册...");
            }
        }
    }
}
