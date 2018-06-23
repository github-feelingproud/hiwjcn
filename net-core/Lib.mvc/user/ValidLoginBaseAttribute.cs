﻿using Lib.helper;
using Lib.mvc.attr;
using Lib.mvc.auth;
using System;
using System.Linq;
using System.Net;

namespace Lib.mvc.user
{
    /// <summary>
    /// 验证登录权限
    /// </summary>
    public abstract class ValidLoginBaseAttribute : _ActionFilterBaseAttribute
    {
        /// <summary>
        /// 权限，逗号隔开
        /// </summary>
        public string Permission { get; set; }
        
        /// <summary>
        /// 获取登录用户
        /// </summary>
        protected virtual LoginUserInfo GetLoginUser(ActionExecutingContext filterContext)
        {
            var context = HttpContext.Current;

            var loginuser = context.GetAuthUser();

            return loginuser;
        }

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

        /// <summary>
        /// 拦截请求
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var loginuser = this.GetLoginUser(filterContext);

            //检查登录
            if (loginuser == null)
            {
                this.WhenNotLogin(ref filterContext);
                return;
            }

            //检查权限
            if (ValidateHelper.IsPlumpString(this.Permission))
            {
                if (this.Permission.Split(',').Where(x => x?.Length > 0).Any(x => !loginuser.HasPermission(x)))
                {
                    this.WhenNoPermission(ref filterContext);
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }

    public class PageAuthAttribute : ValidLoginBaseAttribute
    {
        public override void WhenNoPermission(ref ActionExecutingContext filterContext)
        {
            filterContext.Result = new ViewResult() { ViewName = "~/Views/Shared/Limited.cshtml" };
        }

        public override void WhenNotLogin(ref ActionExecutingContext filterContext)
        {
            var current_url = filterContext.HttpContext.Request.Url.ToString();
            current_url = EncodingHelper.UrlEncode(current_url);
            filterContext.Result = new RedirectResult($"/account/login?continue={current_url}");
        }
    }

    public class ApiAuthAttribute : ValidLoginBaseAttribute
    {
        public override void WhenNoPermission(ref ActionExecutingContext filterContext)
        {
            filterContext.Result = GetJson(new _()
            {
                success = false,
                msg = "没有权限",
                code = (-999).ToString()
            });
        }

        public override void WhenNotLogin(ref ActionExecutingContext filterContext)
        {
            filterContext.Result = GetJson(new _()
            {
                success = false,
                msg = "登录过期，请刷新页面",
                code = (-(int)HttpStatusCode.Unauthorized).ToString()
            });
        }
    }
}
