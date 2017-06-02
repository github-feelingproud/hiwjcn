using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc.attr;
using Lib.extension;
using Lib.helper;
using System.Web.Mvc;
using System.Web;

namespace Lib.mvc.user
{
    public abstract class ValidLoginBaseAttribute : _ActionFilterBaseAttribute
    {
        /// <summary>
        /// 权限，逗号隔开
        /// </summary>
        public string Permission { get; set; }

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

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var context = HttpContext.Current;
            var _loginstatus = this.GetLoginStatus();

            var loginuser = _loginstatus.GetLoginUser(context);

            if (loginuser == null)
            {
                this.WhenNotLogin(ref filterContext);
                return;
            }

            if (ValidateHelper.IsPlumpString(this.Permission))
            {
                if (this.Permission.Split(',').Any(x => !loginuser.HasPermission(x)))
                {
                    this.WhenNoPermission(ref filterContext);
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
