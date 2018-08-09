using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net;

namespace Lib.auth.validator
{
    /// <summary>
    /// 接口，没有登录返回json
    /// </summary>
    public class ApiAuthAttribute : ValidLoginBaseAttribute
    {
        public override void WhenNoPermission(ref ActionExecutingContext filterContext)
        {
            filterContext.Result = GetJson(new _()
            {
                success = false,
                msg = "没有权限",
                code = (-999).ToString()
            });
        }

        public override void WhenNotLogin(ref ActionExecutingContext filterContext)
        {
            filterContext.Result = GetJson(new _()
            {
                success = false,
                msg = "登录过期，请刷新页面",
                code = (-(int)HttpStatusCode.Unauthorized).ToString()
            });
        }
    }
}
