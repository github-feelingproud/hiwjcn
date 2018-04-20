using Lib.core;
using Lib.extension;
using Lib.helper;
using Lib.ioc;
using Lib.mvc.auth;
using Lib.mvc.user;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Configuration;
using System.Net;

namespace Lib.mvc
{
    //[OutputCache(Duration = 10)]
    [ValidateInput(false)]
    public abstract class BaseController : System.Web.Mvc.Controller
    {
        /// <summary>
        /// 访问上下文
        /// </summary>
        public WebWorkContext X { get; private set; }

        public BaseController()
        {
            this.X = new WebWorkContext();
        }

        [NonAction]
        protected virtual int? CheckPage(int? page) => Com.CheckPage(page);

        [NonAction]
        protected virtual int? CheckPageSize(int? size) => Com.CheckPageSize(size);

        #region 返回结果
        /// <summary>
        /// 重写json方法，解决mvc中json丢时区的问题
        /// </summary>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <param name="contentEncoding"></param>
        /// <returns></returns>
        [NonAction]
        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding)
        {
            return this.Json(data, contentType, contentEncoding, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 重写json方法，解决mvc中json丢时区的问题
        /// </summary>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        /// <param name="contentEncoding"></param>
        /// <param name="behavior"></param>
        /// <returns></returns>
        [NonAction]
        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new CustomJsonResult()
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
        }

        /// <summary>
        /// 获取json
        /// </summary>
        [NonAction]
        public virtual ActionResult GetJson(object obj, JsonRequestBehavior behavior = JsonRequestBehavior.AllowGet)
        {
            return Json(obj, behavior);
        }

        /// <summary>
        /// 获取jsonp
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        [NonAction]
        public virtual ActionResult GetJsonp(object obj, string callback = "callback")
        {
            var func = this.Request.QueryString[callback];
            return Content($"{func}({obj.ToJson()})", "text/javascript");
        }

        /// <summary>
        /// 判断是否成功
        /// </summary>
        [NonAction]
        public virtual bool IsSuccess(string msg) => !ValidateHelper.IsPlumpStringAfterTrim(msg);

        /// <summary>
        /// 获取默认的json
        /// </summary>
        [NonAction]
        public virtual ActionResult GetJsonRes(string errmsg = null,
            string code = default(string),
            object data = null)
        {
            return GetJson(new _()
            {
                success = IsSuccess(errmsg),
                msg = errmsg,
                code = code,
                data = data
            });
        }

        /// <summary>
        /// 返回json
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [NonAction]
        public virtual ActionResult StringAsJson(string json)
        {
            return Content(json, "text/json");
        }

        /// <summary>
        /// 系统错误
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public virtual ActionResult Http500()
        {
            return new Http500();
        }

        /// <summary>
        /// 找不到页面
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public virtual ActionResult Http404()
        {
            return new Http404();
        }

        /// <summary>
        /// 永久跳转
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [NonAction]
        public virtual ActionResult Http301(string url)
        {
            return new Http301(url);
        }

        /// <summary>
        /// 没有权限
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public virtual ActionResult Http403()
        {
            return new Http403();
        }

        /// <summary>
        /// 去首页
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public virtual ActionResult GoHome()
        {
            return new GoHomeResult();
        }
        #endregion

        #region action处理

        [NonAction]
        protected virtual ActionResult WhenError(Exception e)
        {
            e.AddLog(this.GetType());

            //捕获的错误
            return GetJsonRes(e.GetInnerExceptionAsJson());
        }

        [NonAction]
        public virtual ActionResult RunAction(Func<ActionResult> GetActionFunc)
        {
            try
            {
                return GetActionFunc.Invoke();
            }
            catch (Exception e)
            {
                return WhenError(e);
            }
        }

        [NonAction]
        public virtual async Task<ActionResult> RunActionAsync(Func<Task<ActionResult>> GetActionFunc)
        {
            try
            {
                return await GetActionFunc.Invoke();
            }
            catch (Exception e)
            {
                return WhenError(e);
            }
        }

        #endregion

    }
}
