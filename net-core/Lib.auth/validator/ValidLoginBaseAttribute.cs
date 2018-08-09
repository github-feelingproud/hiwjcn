using Lib.helper;
using Lib.mvc.attr;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace Lib.auth.validator
{
    public abstract class ValidLoginBaseAttribute : _ActionFilterBaseAttribute
    {
        /// <summary>
        /// 权限，逗号隔开
        /// </summary>
        public string Permission { get; set; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var loginuser = await context.HttpContext.GetAuthUserAsync();

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

            //let it go
            await next.Invoke();
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
}
