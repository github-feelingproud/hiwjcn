using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using Lib.helper;
using Lib.mvc.user;
using Lib.mvc.auth;
using Lib.mvc.auth.validation;
using Lib.mvc;

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

        public string CheckToken() => this.ApiPath("Auth", "CheckToken");
    }

}
