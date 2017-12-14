using Lib.core;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Lib.mvc.attr
{
    /// <summary>
    /// 过滤不是来自本站的请求
    /// </summary>
    public class CheckReferrerUrlAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var reffer = ConvertHelper.GetString(filterContext.HttpContext.Request.UrlReferrer).ToLower();
            var allowlist = ConfigHelper.Instance.AllowDomains;
            if (ValidateHelper.IsPlumpList(allowlist))
            {
                bool find = false;
                foreach (var domain in allowlist)
                {
                    if (reffer.Contains(ConvertHelper.GetString(domain).ToLower()))
                    {
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    filterContext.Result = new ContentResult() { Content = string.Empty };
                    return;
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
