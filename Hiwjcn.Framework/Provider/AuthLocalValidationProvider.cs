using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lib.mvc;
using Lib.mvc.user;
using Lib.helper;
using Lib.core;
using Lib.extension;
using Lib.net;
using Lib.data;
using Lib.cache;
using System.Threading.Tasks;
using Hiwjcn.Core.Infrastructure.Auth;
using Hiwjcn.Bll.Auth;
using Hiwjcn.Core.Domain.Auth;
using Lib.events;
using Hiwjcn.Core.Model.Sys;
using Hiwjcn.Framework.Actors;
using Lib.mvc.auth;
using Hiwjcn.Framework;
using Lib.ioc;
using Hiwjcn.Core;
using Lib.mvc.auth.validation;
using Hiwjcn.Core.Data;
using System.Data.Entity;

namespace Hiwjcn.Framework.Provider
{
    /// <summary>
    /// 查询本地库
    /// </summary>
    public class AuthLocalValidationProvider : TokenValidationProviderBase
    {
        private readonly IAuthTokenToUserService _IAuthTokenToUserService;
        private readonly IValidationDataProvider _dataProvider;

        public AuthLocalValidationProvider(
            IAuthTokenToUserService _IAuthTokenToUserService,
            IValidationDataProvider _dataProvider)
        {
            this._IAuthTokenToUserService = _IAuthTokenToUserService;
            this._dataProvider = _dataProvider;
        }

        public override LoginUserInfo FindUser(HttpContext context)
        {
            return AsyncHelper.RunSync(() => FindUserAsync(context));
        }

        public override async Task<LoginUserInfo> FindUserAsync(HttpContext context)
        {
            try
            {
                var access_token = this._dataProvider.GetToken(context);
                var client_id = this._dataProvider.GetClientID(context);

                var loginuser = await this._IAuthTokenToUserService.FindUserByTokenAsync(access_token, client_id);

                if (!loginuser.success)
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
