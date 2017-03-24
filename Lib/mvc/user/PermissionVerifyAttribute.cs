using Lib.core;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Lib.ioc;

namespace Lib.mvc.user
{
    /// <summary>
    /// 验证用户权限
    /// </summary>
    public class PermissionVerifyAttribute : ActionFilterAttribute
    {
        public string ReDirectUrl { get; set; }

        public string Permission { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            SSOClientHelper.CheckSSOConfig();

            var context = HttpContext.Current;

            var loginuser = AppContext.GetObject<LoginStatus>().GetLoginUser(context);

            //==============================================================================

            if (loginuser == null)
            {
                filterContext.Result = GetJson(new _() { success = false, msg = "未登录" });
                return;
            }
            //验证权限
            if (Permission?.Length > 0)
            {
                foreach (var p in Permission.Split(',').Where(x => x?.Length > 0))
                {
                    if (!loginuser.HasPermission(p))
                    {
                        if (ReDirectUrl?.Length > 0)
                        {
                            filterContext.Result = new RedirectResult(ReDirectUrl);
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


        private ActionResult GetJson(object data)
        {
            return new JsonResult()
            {
                Data = data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}
