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
        private readonly LoginStatus _loginstatus;

        public AuthLocalValidationProvider(
            IAuthTokenToUserService _IAuthTokenToUserService,
            IValidationDataProvider _dataProvider,
            LoginStatus _loginstatus)
        {
            this._IAuthTokenToUserService = _IAuthTokenToUserService;
            this._dataProvider = _dataProvider;
            this._loginstatus = _loginstatus;
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

        public override void WhenUserNotLogin(HttpContext context)
        {
            this._loginstatus.SetUserLogout(context);
        }

        public override void WhenUserLogin(HttpContext context, LoginUserInfo loginuser)
        {
            this._loginstatus.SetUserLogin(context, loginuser);
        }
    }

    public class SSOValidationProvider : TokenValidationProviderBase
    {
        private readonly LoginStatus ls;

        public SSOValidationProvider()
        {
            this.ls = AccountHelper.SSO;
        }

        public override string HttpItemKey() => "httpcontext.item.sso.user.entity";

        public override LoginUserInfo FindUser(HttpContext context)
        {
            try
            {
                var uid = ls.GetCookieUID(context);
                var token = ls.GetCookieToken(context);
                if (!ValidateHelper.IsAllPlumpString(uid, token))
                {
                    return null;
                }
                using (var db = new SSODB())
                {
                    var model = db.T_UserInfo.Where(x => x.UID == uid).FirstOrDefault();
                    if (model == null || model.CreateToken() != token)
                    {
                        return null;
                    }
                    //load permission
                    //这里只拿了角色关联的权限，部门关联的权限没有拿
                    var roleslist = db.Auth_UserRole.Where(x => x.UserID == uid)
                        .Select(x => x.RoleID).ToList()
                        .Select(x => $"role:{x}").ToList();

                    model.Permissions = db.Auth_PermissionMap.Where(x => roleslist.Contains(x.MapKey))
                        .Select(x => x.PermissionID).ToList()
                        .Distinct().ToList();

                    return model.LoginUserInfo();
                }
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

        public override void WhenUserLogin(HttpContext context, LoginUserInfo loginuser)
        {
            ls.SetUserLogin(context, loginuser);
        }

        public override void WhenUserNotLogin(HttpContext context)
        {
            ls.SetUserLogout(context);
        }
    }
}
