using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Lib.ioc;
using Lib.helper;
using Lib.core;
using Lib.extension;
using Lib.mvc.user;
using System.Net;
using System.Net.Http;
using Lib.net;

namespace Lib.mvc.auth.validation
{
    public abstract class TokenValidationProviderBase
    {
        public abstract LoginUserInfo FindUser(HttpContext context);

        public virtual async Task<LoginUserInfo> FindUserAsync(HttpContext context) => await Task.FromResult(this.FindUser(context));
    }

    /// <summary>
    /// 直接使用login status
    /// </summary>
    public class CookieTokenValidationProvider : TokenValidationProviderBase
    {
        private readonly LoginStatus _LoginStatus;

        public CookieTokenValidationProvider(LoginStatus _LoginStatus)
        {
            this._LoginStatus = _LoginStatus;
        }

        public override LoginUserInfo FindUser(HttpContext context)
        {
            return this._LoginStatus.GetLoginUser(context);
        }
    }

    class TokenReturn
    {
        public string token { get; set; }
        public string refresh_token { get; set; }
        public DateTime expire { get; set; }
        public string user_uid { get; set; }
        public List<string> scope { get; set; }
    }

    /// <summary>
    /// 请求auth server验证
    /// </summary>
    public class AuthServerValidationProvider : TokenValidationProviderBase
    {
        private static readonly HttpClient _client = new HttpClient();

        private readonly AuthServerConfig _server;

        public AuthServerValidationProvider(AuthServerConfig server)
        {
            this._server = server;
        }

        public override LoginUserInfo FindUser(HttpContext context)
        {
            try
            {
                var token = context.GetBearerToken();
                var client_id = context.Request.Headers["client_id"];
                if (!ValidateHelper.IsAllPlumpString(token, client_id))
                {
                    return null;
                }

                var json = HttpClientHelper.Post_(this._server.ApiPath("auth", "CheckToken"), new
                {
                    client_id = client_id,
                    access_token = token
                });
                var data = json.JsonToEntity<TokenReturn>();

                return new LoginUserInfo()
                {
                    UserID = data.user_uid,
                    LoginToken = data.token,
                    RefreshToken = data.refresh_token,
                    TokenExpire = data.expire,
                    Scopes = data.scope
                };
            }
            catch (Exception e)
            {
                e.AddErrorLog();
                return null;
            }
        }

        public override async Task<LoginUserInfo> FindUserAsync(HttpContext context)
        {
            return await Task.FromResult(this.FindUser(context));
        }
    }
}
