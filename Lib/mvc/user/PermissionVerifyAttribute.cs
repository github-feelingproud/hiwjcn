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
    public class PermissionVerifyAttribute : _ActionFilterBaseAttribute
    {
        /// <summary>
        /// 没有权限跳转的URL
        /// </summary>
        public string NoPermissionUrl { get; set; }
        /// <summary>
        /// 权限检查
        /// </summary>
        public string Permission { get; set; }

        public override LoginStatus GetLoginStatus()
        {
            return AppContext.GetObject<LoginStatus>();
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var loginuser = GetLoginStatus().GetLoginUser(context);
            //==============================================================================

            if (loginuser == null)
            {
                filterContext.Result = GetJson(new _() { success = false, msg = "未登录" });
                return;
            }
            //验证权限
            if (Permission?.Length > 0)
            {
                if (Permission.Split(',').Where(x => x?.Length > 0).Any(x => !loginuser.HasPermission(x)))
                {
                    if (NoPermissionUrl?.Length > 0)
                    {
                        filterContext.Result = new RedirectResult(NoPermissionUrl);
                    }
                    else
                    {
                        filterContext.Result = GetJson(new _() { success = false, msg = "没有权限" });
                    }
                    return;
                }
            }
        }
    }
}
