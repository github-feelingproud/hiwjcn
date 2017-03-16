using Lib.core;
using Lib.extension;
using Lib.helper;
using Lib.ioc;
using Lib.mvc.user;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

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
            X = new WebWorkContext();
        }

        [NonAction]
        protected int? CheckPage(int? page)
        {
            return Com.CheckPage(page);
        }

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
        /// <param name="obj"></param>
        /// <returns></returns>
        [NonAction]
        public ActionResult GetJson(object obj)
        {
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取默认的json
        /// </summary>
        /// <param name="errmsg"></param>
        /// <returns></returns>
        [NonAction]
        public ActionResult GetJsonRes(string errmsg)
        {
            return GetJson(new ResJson() { success = !ValidateHelper.IsPlumpString(errmsg), msg = errmsg });
        }

        /// <summary>
        /// 返回json
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [NonAction]
        public ActionResult StringAsJson(string json)
        {
            return Content(json, "text/json");
        }

        /// <summary>
        /// 系统错误
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public ActionResult Http500()
        {
            return new Http500();
        }

        /// <summary>
        /// 找不到页面
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public ActionResult Http404()
        {
            return new Http404();
        }

        /// <summary>
        /// 永久跳转
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [NonAction]
        public ActionResult Http301(string url)
        {
            return new Http301(url);
        }

        /// <summary>
        /// 没有权限
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public ActionResult Http403()
        {
            return new Http403();
        }

        /// <summary>
        /// 去首页
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public ActionResult GoHome()
        {
            return new GoHomeResult();
        }
        #endregion

        #region action处理
        /// <summary>
        /// 返回错误页面或者json
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        [NonAction]
        private ActionResult GetErrorResult(string msg)
        {
            if (this.X.IsAjax || this.X.IsPost)
            {
                return this.GetJsonRes(msg);
            }
            return this.GetJsonRes(msg);
        }

        /// <summary>
        /// 获取action的时候捕获异常
        /// </summary>
        /// <param name="GetActionFunc"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        [NonAction]
        public ActionResult RunAction(Func<ActionResult> GetActionFunc, ActionResult ErrorResult = null)
        {
            try
            {
                return GetActionFunc.Invoke();
            }
            catch (Exception e)
            {
                e.SaveLog(this.GetType());
                if (ErrorResult != null)
                {
                    return ErrorResult;
                }
                else
                {
                    return GetErrorResult(e.GetInnerExceptionAsJson());
                }
            }
        }

        /// <summary>
        /// 异步，还没有使用
        /// </summary>
        /// <param name="GetActionFunc"></param>
        /// <param name="controller"></param>
        /// <param name="ErrorResult"></param>
        /// <returns></returns>
        [NonAction]
        public async Task<ActionResult> RunActionAsync(Func<Task<ActionResult>> GetActionFunc, ActionResult ErrorResult = null)
        {
            try
            {
                return await GetActionFunc.Invoke();
            }
            catch (Exception e)
            {
                e.SaveLog(this.GetType());
                if (ErrorResult != null)
                {
                    return ErrorResult;
                }
                else
                {
                    return GetErrorResult(e.GetInnerExceptionAsJson());
                }
            }
        }

        /// <summary>
        /// 通过session验证身份
        /// 里面不要捕获异常，此方法会自动记录日志
        /// loginuser为有效登陆用户，用户ID>0
        /// </summary>
        /// <param name="GetActionFunc"></param>
        /// <param name="controller"></param>
        /// <param name="loginuser">有效登陆用户</param>
        /// <param name="ForAdminLogin"></param>
        /// <param name="NeedRoleList"></param>
        /// <param name="NoPermissionResult"></param>
        /// <param name="DefaultErrorResult"></param>
        /// <returns></returns>
        [NonAction]
        public ActionResult RunActionWhenLogin(
            Func<LoginUserInfo, ActionResult> GetActionFunc, List<string> PermissionList = null,
            ActionResult NoPermissionResult = null, ActionResult ErrorResult = null)
        {
            return RunAction(() =>
            {
                LoginUserInfo loginuser = null;
                //尝试通过token获取登陆用户
                var token = this.Request.Headers["token"];
                if (ValidateHelper.IsPlumpString(token))
                {
                    loginuser = null;
                }
                //尝试使用session和cookie获取登陆用户
                if (loginuser == null)
                {
                    loginuser = this.X.LoginUser;
                }

                //如果没登陆就跳转
                if (loginuser == null)
                {
                    //没有登陆就跳转登陆
                    var redirect_url = AppContext.GetObject<IGetLoginUrl>().GetUrl(this.X.Url);
                    return new RedirectResult(redirect_url);
                }
                //所需要的全部权限值
                foreach (var per in ConvertHelper.NotNullList(PermissionList))
                {
                    //只要没有满足一个权限就返回错误值
                    if (!loginuser.HasPermission(per))
                    {
                        if (NoPermissionResult != null)
                        {
                            return NoPermissionResult;
                        }
                        else
                        {
                            return this.GetJsonRes("没有权限");
                        }
                    }
                }

                return GetActionFunc.Invoke(loginuser);
            }, ErrorResult);
        }
        #endregion

    }
}
