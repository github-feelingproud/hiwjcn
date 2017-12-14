using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Lib.mvc.attr
{
    /// <summary>
    /// 标记接口过期时间
    /// </summary>
    public class ApiExpireAtAttribute : ActionFilterAttribute
    {
        private DateTime Date { get; set; }

        public ApiExpireAtAttribute(string date)
        {
            this.Date = DateTime.Parse(date);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (DateTime.Now > this.Date)
            {
                filterContext.Result = ResultHelper.BadRequest("无法响应请求，请升级客户端");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
