using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using Lib.helper;

namespace Lib.mvc.auth
{
    public class AuthServerConfig
    {
        public string ServerUrl { get; set; }

        public string ApiPath(params string[] paths)
        {
            var path = "/".Join_(paths.Where(x => ValidateHelper.IsPlumpString(x)));
            return ServerUrl.EnsureTrailingSlash() + path;
        }
    }

    public class AuthConfig
    {
        public Func<ITokenProvider> TokenProvider { get; set; }

        public Func<IAuthStoreProvider> AuthStoreProvider { get; set; }

        public Func<IAuthTokenCheckProvider> AuthTokenCheckProvider { get; set; }
    }

}
