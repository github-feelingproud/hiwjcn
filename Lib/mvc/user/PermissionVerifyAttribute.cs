using Lib.core;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

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

            var user = AccountHelper.User.GetLoginUser(context);
            if (user == null)
            {
                //没有登陆就跳转登陆
                var cur_url = context.Server.UrlEncode(context.Request.Url.ToString());
                var cb_url = context.Server.UrlEncode(ConfigHelper.Instance.CallBackUrl);
                var redirect_url = $"{ConfigHelper.Instance.SSOLoginUrl}?url={cur_url}&callback={cb_url}";
                filterContext.Result = new RedirectResult(redirect_url);
                return;
            }
            //验证权限
            if (Permission?.Length > 0)
            {
                foreach (var p in Permission.Split(',').Where(x => x?.Length > 0))
                {
                    if (!user.HasPermission(p))
                    {
                        ActionResult re = null;
                        if (ReDirectUrl?.Length > 0)
                        {
                            re = new RedirectResult(ReDirectUrl);
                        }
                        else
                        {
                            re = new JsonResult()
                            {
                                Data = new ResJson() { Success = false, ErrorMsg = "没有权限", ErrorCode = p },
                                JsonRequestBehavior = JsonRequestBehavior.AllowGet
                            };
                        }
                        filterContext.Result = re;
                        return;
                    }
                }
            }
        }

    }
}
