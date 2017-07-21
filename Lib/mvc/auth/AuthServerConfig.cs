using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.mvc.auth
{
    public class AuthServerConfig
    {
    }

    public class AuthConfig
    {
        public Func<ITokenProvider> TokenProvider { get; set; }

        public Func<IAuthStoreProvider> AuthStoreProvider { get; set; }

        public Func<IAuthTokenCheckProvider> AuthTokenCheckProvider { get; set; }
    }

}
