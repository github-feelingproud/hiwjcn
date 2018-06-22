using Lib.cache;
using Lib.core;
using Lib.extension;
using Lib.ioc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lib.mvc.attr
{
    /// <summary>
    /// 防止重复提交
    /// </summary>
    public class AntiReSubmitAttribute : _ActionFilterBaseAttribute
    {
        public virtual int CacheSeconds { get; set; } = 5;

        public virtual string ErrorMessage { get; set; } = "重复提交，请稍后再试";

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var sessionID = context.HttpContext.Session.Id;
            var key = $"{nameof(AntiReSubmitAttribute)}:{sessionID}";

            var reqparams = context.HttpContext.Request.Form.ToDict();
            reqparams = reqparams.AddDict(context.HttpContext.Request.Query.ToDict());

            var dict = new SortedDictionary<string, string>(reqparams, new MyStringComparer());
            var submitData = dict.ToUrlParam();
            var (AreaName, ControllerName, ActionName) = context.RouteData.GetRouteInfo();
            submitData = $"{AreaName}/{ControllerName}/{ActionName}/:{submitData}";
            //读取缓存
            using (var s = IocContext.Instance.Scope())
            {
                using (var cache = s.Resolve_<ICacheProvider>())
                {
                    var data = cache.Get<string>(key);
                    if (data.Success)
                    {
                        if (data.Result == submitData)
                        {
                            context.Result = ResultHelper.BadRequest(this.ErrorMessage);
                            return;
                        }
                    }
                    //10秒钟不能提交相同的数据
                    CacheSeconds = Math.Abs(CacheSeconds);
                    if (CacheSeconds == 0) { throw new Exception("缓存时间不能为0"); }
                    cache.Set(key, submitData, TimeSpan.FromSeconds(CacheSeconds));
                }
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
