using Lib.core;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Lib.ioc;
using Lib.mvc.attr;

namespace Lib.mvc.user
{
    /// <summary>
    /// 验证用户权限
    /// </summary>
    public class PermissionVerifyAttribute : ValidLoginBaseAttribute
    {
        /// <summary>
        /// 没有权限跳转的URL
        /// </summary>
        public string NoPermissionUrl { get; set; }
        
        /// <summary>
        /// 没有权限
        /// </summary>
        /// <param name="filterContext"></param>
        public override void WhenNoPermission(ref ActionExecutingContext filterContext)
        {
            if (NoPermissionUrl?.Length > 0)
            {
                filterContext.Result = new RedirectResult(NoPermissionUrl);
            }
            else
            {
                filterContext.Result = GetJson(new _() { success = false, msg = "没有权限" });
            }
        }

        /// <summary>
        /// 没有登录
        /// </summary>
        /// <param name="filterContext"></param>
        public override void WhenNotLogin(ref ActionExecutingContext filterContext)
        {
            filterContext.Result = GetJson(new _() { success = false, msg = "未登录" });
        }
    }
}
