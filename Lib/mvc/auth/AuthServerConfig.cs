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
        public readonly string ServerUrl;

        public AuthServerConfig(string host)
        {
            this.ServerUrl = ConvertHelper.GetString(host);

            var cp = this.ServerUrl.ToLower();
            if (!(cp.StartsWith("http://") || cp.StartsWith("https://")))
            {
                throw new Exception("auth服务器地址必须以http或者https开头");
            }
        }

        public string ApiPath(params string[] paths)
        {
            var path = "/".Join_(paths.Where(x => ValidateHelper.IsPlumpString(x)));
            return ServerUrl.EnsureTrailingSlash() + path;
        }

        /// <summary>
        /// 获取code
        /// </summary>
        public string CreateAuthCodeByPassword() => this.ApiPath("Auth", "AuthCodeByPassword");

        /// <summary>
        /// 获取code
        /// </summary>
        public string CreateCodeByOneTimeCode() => this.ApiPath("Auth", "AuthCodeByOneTimeCode");

        /// <summary>
        /// 获取token
        /// </summary>
        public string CreateToken() => this.ApiPath("Auth", "AccessToken");

        /// <summary>
        /// token换用户信息
        /// </summary>
        public string CheckToken() => this.ApiPath("Auth", "CheckToken");

    }

}
