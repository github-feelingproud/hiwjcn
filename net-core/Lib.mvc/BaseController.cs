using Lib.extension;
using Lib.helper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Lib.mvc
{
    public abstract class BaseController : Controller
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
        [NonAction]
        public override JsonResult Json(object data)
        {
            return base.Json(data, JsonHelper._setting);
        }

        [NonAction]
        public virtual ActionResult GetJson(object obj) => this.Json(obj);

        [NonAction]
        public virtual ActionResult GetJsonp(object obj, string callback = "callback")
        {
            var func = (string)this.HttpContext.Request.Query[callback];

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
