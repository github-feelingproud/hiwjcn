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
    /// <summary>
    /// 拿到token和client信息后去验证信息，并拿到用户信息
    /// </summary>
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

    /// <summary>
    /// 请求auth server验证
    /// </summary>
    public class AuthServerValidationProvider : TokenValidationProviderBase
    {
        private readonly AuthServerConfig _server;
        private readonly IValidationDataProvider _dataProvider;

        public AuthServerValidationProvider(AuthServerConfig server, IValidationDataProvider _dataProvider)
        {
            this._server = server;
            this._dataProvider = _dataProvider;
        }

        public override LoginUserInfo FindUser(HttpContext context)
        {
            try
            {
                var token = this._dataProvider.GetToken(context);
                var client_id = this._dataProvider.GetClientID(context);
                if (!ValidateHelper.IsAllPlumpString(token, client_id))
                {
                    $"token和client_id为空:{token}-{client_id}".AddBusinessInfoLog();
                    return null;
                }

                var json = HttpClientHelper.PostJson(this._server.CheckToken(), new
                {
                    client_id = client_id,
                    access_token = token
                });
                var data = json.JsonToEntity<_<LoginUserInfo>>();
                if (!data.success)
                {
                    $"check token返回数据:{data.ToJson()}".AddBusinessInfoLog();
                    return null;
                }
                return data.data ?? throw new Exception("服务器返回数据为空-769876");
            }
            catch (Exception e)
            {
                e.AddErrorLog();
                return null;
            }
        }

        public override async Task<LoginUserInfo> FindUserAsync(HttpContext context)
        {
            try
            {
                var token = this._dataProvider.GetToken(context);
                var client_id = this._dataProvider.GetClientID(context);
                if (!ValidateHelper.IsAllPlumpString(token, client_id))
                {
                    $"token和client_id为空:{token}-{client_id}".AddBusinessInfoLog();
                    return null;
                }

                var caller = new AuthServerApiCaller(this._server);
                var data = await caller.CheckToken(client_id, token);
                if (!data.success)
                {
                    $"check token返回数据:{data.ToJson()}".AddBusinessInfoLog();
                    return null;
                }
                return data.data;
            }
            catch (Exception e)
            {
                e.AddErrorLog();
                return null;
            }
        }
    }
}
