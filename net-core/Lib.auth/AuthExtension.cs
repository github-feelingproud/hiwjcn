using Lib.extension;
using Lib.helper;
using Lib.ioc;
using Lib.mvc.auth.validation;
using Lib.mvc.user;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Lib.mvc.auth
{
    public static class AuthExtension
    {
        /// <summary>
        /// 获取当前登录用户
        /// </summary>
        public static async Task<LoginUserInfo> GetAuthUserAsync(this HttpContext context, string name = null)
        {
            using (var x = IocContext.Instance.Scope())
            {
                var loginuser = await x.Resolve_<ITokenValidationProvider>().GetLoginUserInfoAsync(context);

                return loginuser;
            }
        }

        /// <summary>
        /// 获取bearer token
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetBearerToken(this HttpContext context)
        {
            var bearer = "Bearer" + ' '.ToString();
            var token = ((string)context.Request.Headers["Authorization"]) ?? string.Empty;
            if (token.StartsWith(bearer, StringComparison.OrdinalIgnoreCase))
            {
                return token.Substring(bearer.Length);
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取bearer token或者header.auth.token
        /// </summary>
        public static string GetAuthToken(this HttpContext context)
        {
            var tk = context.GetBearerToken();
            if (!ValidateHelper.IsPlumpString(tk))
            {
                tk = context.Request.Headers["auth.token"];
            }
            return tk;
        }

        /// <summary>
        /// 使用cookie登录
        /// </summary>
        public static void CookieLogin(this HttpContext context, LoginUserInfo loginuser)
        {
            using (var s = IocContext.Instance.Scope())
            {
                var loginstatus = s.Resolve_<LoginStatus>();
                loginstatus.SetUserLogin(context, loginuser);
            }
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        public static void CookieLogout(this HttpContext context)
        {
            using (var s = IocContext.Instance.Scope())
            {
                var loginstatus = s.Resolve_<LoginStatus>();
                loginstatus.SetUserLogout(context);
            }
        }

        /// <summary>
        /// 获取这个程序集中所用到的所有权限
        /// </summary>\
        public static List<string> ScanAllAssignedPermissionOnThisAssembly(this Assembly ass)
        {
            var permission_list = new List<string>();
            var tps = ass.GetTypes();
            tps = tps.Where(x => x.IsNormalClass() && x.IsAssignableTo_<Controller>()).ToArray();
            foreach (var t in tps)
            {
                var methods = t.GetMethods().Where(x => x.IsPublic);
                foreach (var m in methods)
                {
                    var attr = m.GetCustomAttribute<ValidLoginBaseAttribute>();
                    if (attr == null) { continue; }
                    var pers = attr.Permission?.Split(',').ToList();
                    if (ValidateHelper.IsPlumpList(pers))
                    {
                        permission_list.AddRange(pers);
                    }
                }
            }
            permission_list = permission_list.Distinct().Where(x => ValidateHelper.IsPlumpString(x)).ToList();

            return permission_list;
        }
    }
}
