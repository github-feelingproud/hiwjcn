using Akka.Actor;
using Hiwjcn.Core.Domain.Sys;
using Hiwjcn.Framework.Actors;
using Lib.distributed.akka;
using Lib.extension;
using Lib.helper;
using Lib.mvc;
using System;
using System.Web;
using System.Web.Mvc;

namespace Hiwjcn.Framework
{
    /// <summary>
    /// 记录请求
    /// </summary>
    public class RequestLogAttribute : ActionFilterAttribute
    {
        private DateTime start { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            start = DateTime.Now;
            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            try
            {
                var context = HttpContext.Current;

                var model = new ReqLogEntity();

                model.ReqTime = (DateTime.Now - start).TotalMilliseconds;
                model.ReqID = Com.GetRequestID();

                model.ReqRefURL = ConvertHelper.GetString(context.Request.UrlReferrer);
                model.ReqURL = ConvertHelper.GetString(context.Request.Url);

                var route = filterContext.RouteData.GetA_C_A();

                model.AreaName = route.area?.ToLower();
                model.ControllerName = route.controller?.ToLower();
                model.ActionName = route.action?.ToLower();

                model.ReqMethod = filterContext.HttpContext.Request.HttpMethod;

                model.PostParams = context.Request.Form.ToDict().ToUrlParam();
                model.GetParams = context.Request.QueryString.ToDict().ToUrlParam();

                ActorsManager<LogRequestActor>.Instance.DefaultClient.Tell(model);

                //Com.Range(3).ForEach_(x => AkkaHelper<TestActor>.Tell($"测试actor内存占用{DateTime.Now}"));
            }
            catch (Exception e)
            {
                e.AddLog(this.GetType());
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
