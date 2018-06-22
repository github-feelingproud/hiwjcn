using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lib.mvc.attr
{
    /// <summary>
    /// 标记接口过期时间
    /// </summary>
    public class ApiExpireAtAttribute : _ActionFilterBaseAttribute
    {
        private DateTime Date { get; set; }

        public ApiExpireAtAttribute(string date)
        {
            this.Date = DateTime.Parse(date);
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (DateTime.Now > this.Date)
            {
                context.Result = ResultHelper.BadRequest("无法响应请求，请升级客户端");
                return;
            }
            await base.OnActionExecutionAsync(context, next);
        }
    }
}
