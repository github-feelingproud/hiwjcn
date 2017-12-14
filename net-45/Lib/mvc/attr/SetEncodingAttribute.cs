using Lib.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Lib.mvc.attr
{
    /// <summary>
    /// 设置编码
    /// </summary>
    public class SetEncodingAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var config = ConfigHelper.Instance;
            filterContext.HttpContext.Response.ContentEncoding = config.SystemEncoding;
            filterContext.HttpContext.Request.ContentEncoding = config.SystemEncoding;
            base.OnActionExecuting(filterContext);
        }
    }
}
