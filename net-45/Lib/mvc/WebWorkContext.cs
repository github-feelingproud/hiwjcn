using Lib.mvc.user;
using Lib.ioc;
using System;
using System.Web;
using Lib.mvc.auth;
using System.Threading.Tasks;

namespace Lib.mvc
{
    /// <summary>
    /// PC前台工作上下文类
    /// </summary>
    public class WebWorkContext : IDisposable
    {
        public HttpContext context { get; private set; }

        public bool IsPost { get; private set; }

        public bool IsAjax { get; private set; }

        public bool IsPostAjax
        {
            get => this.IsPost && this.IsAjax;
        }

        public string IP { get; private set; }

        public string BaseUrl { get; private set; }

        public string Url { get; private set; }

        public WebWorkContext() : this(System.Web.HttpContext.Current)
        { }

        public WebWorkContext(System.Web.HttpContext context)
        {
            this.context = context ?? throw new Exception("上下文不能为空");

            this.IsPost = context.Request.IsPost();
            this.IsAjax = context.Request.IsAjax();
            this.IP = context.Request.GetCurrentIpAddress();
            this.BaseUrl = context.Request.GetBaseUrl();
            this.Url = context.Request.GetCurrentUrl();
        }

        public void Dispose()
        {
            //
        }
    }
}
