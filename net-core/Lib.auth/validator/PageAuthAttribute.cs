using Lib.helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lib.auth.validator
{

    /// <summary>
    /// 页面，没有登录跳转
    /// </summary>
    public class PageAuthAttribute : ValidLoginBaseAttribute
    {
        public override void WhenNoPermission(ref ActionExecutingContext filterContext)
        {
            filterContext.Result = new ViewResult() { ViewName = "~/Views/Shared/Limited.cshtml" };
        }

        public override void WhenNotLogin(ref ActionExecutingContext filterContext)
        {
            var current_url = filterContext.HttpContext.Request.Path.ToString();
            current_url = EncodingHelper.UrlEncode(current_url);
            filterContext.Result = new RedirectResult($"/account/login?continue={current_url}");
        }
    }
}
