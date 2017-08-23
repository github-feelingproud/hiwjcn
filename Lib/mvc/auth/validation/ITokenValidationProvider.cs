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
        public virtual string HttpItemKey() => "context.items.auth.user.entity";

        [Obsolete("不要直接调用，使用哪个有缓存的")]
        public abstract LoginUserInfo FindUser(HttpContext context);

        [Obsolete("不要直接调用，使用哪个有缓存的")]
        public abstract Task<LoginUserInfo> FindUserAsync(HttpContext context);

        public virtual void WhenUserNotLogin(HttpContext context)
        {
            AppContext.Scope(s =>
            {
                s.ResolveOptional_<LoginStatus>()?.SetUserLogout(context);
                return true;
            });
        }

        public virtual void WhenUserLogin(HttpContext context, LoginUserInfo loginuser)
        {
            AppContext.Scope(s =>
            {
                s.ResolveOptional_<LoginStatus>()?.SetUserLogin(context, loginuser);
                return true;
            });
        }

        public LoginUserInfo GetLoginUserInfo(HttpContext context)
        {
            var data = context.CacheInHttpContext(this.HttpItemKey(), () =>
            {
                var loginuser = this.FindUser(context);
                if (loginuser == null)
                {
                    this.WhenUserNotLogin(context);
                }
                else
                {
                    this.WhenUserLogin(context, loginuser);
                }

                return loginuser;
            });
            return data;
        }

        public async Task<LoginUserInfo> GetLoginUserInfoAsync(HttpContext context)
        {
            var data = await context.CacheInHttpContextAsync(this.HttpItemKey(), async () =>
            {
                var loginuser = await this.FindUserAsync(context);
                if (loginuser == null)
                {
                    this.WhenUserNotLogin(context);
                }
                else
                {
                    this.WhenUserLogin(context, loginuser);
                }

                return loginuser;
            });
            return data;
        }

    }

    /// <summary>
    /// 请求auth server验证
    /// </summary>
    public class AuthServerValidationProvider : TokenValidationProviderBase
    {
        private readonly AuthServerConfig _server;
        private readonly IValidationDataProvider _dataProvider;
        public readonly LoginStatus _loginstatus;

        public AuthServerValidationProvider(
            AuthServerConfig server,
            IValidationDataProvider _dataProvider,
            LoginStatus _loginstatus)
        {
            this._server = server;
            this._dataProvider = _dataProvider;
            this._loginstatus = _loginstatus;
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
                return data.data;
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

        public override void WhenUserNotLogin(HttpContext context)
        {
            this._loginstatus.SetUserLogout(context);
        }

        public override void WhenUserLogin(HttpContext context, LoginUserInfo loginuser)
        {
            this._loginstatus.SetUserLogin(context, loginuser);
        }
    }
}
