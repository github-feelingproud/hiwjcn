using Lib.helper;
using Lib.mvc.attr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Lib.auth.validator
{
    public abstract class ValidLoginBaseAttribute : _ActionFilterBaseAttribute
    {
        /// <summary>
        /// 权限，逗号隔开
        /// </summary>
        public string Permission { get; set; }

        protected virtual async Task<LoginUserInfo> GetLoginUser(ActionExecutingContext filterContext)
        {
            var context = filterContext.HttpContext;

            var loginuser = await context.GetAuthUserAsync();

            return loginuser;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var loginuser = await this.GetLoginUser(context);

            //检查登录
            if (loginuser == null)
            {
                this.WhenNotLogin(ref context);
                return;
            }

            //检查权限
            if (ValidateHelper.IsPlumpString(this.Permission))
            {
                if (this.Permission.Split(',').Where(x => x?.Length > 0).Any(x => !loginuser.HasPermission(x)))
                {
                    this.WhenNoPermission(ref context);
                    return;
                }
            }
        }

        /// <summary>
        /// 没有登录的时候调用
        /// </summary>
        /// <param name="filterContext"></param>
        public abstract void WhenNotLogin(ref ActionExecutingContext filterContext);

        /// <summary>
        /// 没有权限的时候调用
        /// </summary>
        /// <param name="filterContext"></param>
        public abstract void WhenNoPermission(ref ActionExecutingContext filterContext);
    }

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
