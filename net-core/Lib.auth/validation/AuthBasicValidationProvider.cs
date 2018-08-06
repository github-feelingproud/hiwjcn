using Lib.core;
using Lib.extension;
using Lib.helper;
using Lib.mvc.user;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Lib.mvc.auth.validation
{
    /// <summary>
    /// 使用了auth api来验证
    /// </summary>
    public class AuthBasicValidationProvider : TokenValidationProviderBase
    {
        private readonly IAuthDataProvider _dataProvider;
        private readonly IAuthApi api;

        public AuthBasicValidationProvider(
            IAuthDataProvider _dataProvider,
            IAuthApi api)
        {
            this._dataProvider = _dataProvider;
            this.api = api;
        }

        public override string HttpContextItemKey() => "context.items.auth.user.entity";

        public override LoginUserInfo FindUser(HttpContext context)
        {
            return AsyncHelper.RunSync(() => FindUserAsync(context));
        }

        public override async Task<LoginUserInfo> FindUserAsync(HttpContext context)
        {
            try
            {
                var access_token = this._dataProvider.GetToken(context);

                var loginuser = await this.api.GetLoginUserInfoByTokenAsync(access_token: access_token);

                if (loginuser.error)
                {
                    loginuser.msg?.AddBusinessInfoLog();
                    return null;
                }

                return loginuser.data;
            }
            catch (Exception e)
            {
                e.AddErrorLog();
                return null;
            }
        }
    }
}
