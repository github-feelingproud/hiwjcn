using Bll;
using Bll.Category;
using Hiwjcn.Bll.Sys;
using Hiwjcn.Core.Model.Sys;
using Lib.helper;
using Lib.core;
using Lib.http;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Lib.cache;
using System.Net;
using Lib.mvc;
using Lib.ioc;
using Lib.mvc.user;
using Lib.extension;

namespace Hiwjcn.Framework
{
    /// <summary>
    /// 在action之前加载导航数据
    /// </summary>
    public class LoadNavigationAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 是否使用缓存
        /// </summary>
        public bool UseCache { get; set; }

        /// <summary>
        /// 在action之前加载导航数据
        /// </summary>
        /// <param name="cache"></param>
        public LoadNavigationAttribute(bool cache = true)
        {
            UseCache = cache;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var nav_key = "nav_list";

            var bll = new CategoryBll() { UseCache = UseCache };

            filterContext.Controller.ViewData[nav_key] = bll.GetCategoryByType(nav_key, maxCount: 500);

            base.OnActionExecuting(filterContext);
        }
    }

    /// <summary>
    /// 记录请求
    /// </summary>
    public class RequestLogAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// form和querystring转换为字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string NameValueToParamString(NameValueCollection data)
        {
            var dict = new Dictionary<string, string>();
            for (var i = 0; i < data.Keys.Count; ++i)
            {
                var key = data.Keys[i];
                dict[key] = data[key];
            }
            return Com.DictToUrlParams(dict);
        }

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
                var model = new ReqLogModel();

                model.ReqTime = (DateTime.Now - start).TotalSeconds;
                model.ReqID = Com.GetRequestID();

                model.ReqRefURL = ConvertHelper.GetString(filterContext.HttpContext.Request.UrlReferrer);
                model.ReqURL = ConvertHelper.GetString(filterContext.HttpContext.Request.Url);

                model.AreaName = ConvertHelper.GetString(filterContext.RouteData.Values["Area"]);
                model.ControllerName = ConvertHelper.GetString(filterContext.RouteData.Values["Controller"]);
                model.ActionName = ConvertHelper.GetString(filterContext.RouteData.Values["Action"]);

                model.ReqMethod = ConvertHelper.GetString(filterContext.HttpContext.Request.HttpMethod);

                model.PostParams = NameValueToParamString(filterContext.HttpContext.Request.Form);
                model.GetParams = NameValueToParamString(filterContext.HttpContext.Request.QueryString);

                model.UpdateTime = DateTime.Now;

                new ReqLogBll().AddLog(model);
            }
            catch (Exception e)
            {
                e.AddLog(this.GetType());
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
