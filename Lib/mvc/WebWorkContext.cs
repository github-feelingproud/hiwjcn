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
            get
            {
                return IsPost && IsAjax;
            }
        }

        public string IP { get; private set; }

        public string BaseUrl { get; private set; }

        public string Url { get; private set; }

        #region 登录信息
        private LoginUserInfo _user;
        public LoginUserInfo User
        {
            get
            {
                if (this._user == null)
                {
                    this._user = this.context.GetAuthUser();
                }
                return this._user;
            }
        }

        public LoginUserInfo SSOUser { get; private set; }

        public LoginUserInfo LoginUser { get; private set; }

        public LoginUserInfo LoginTrader { get; private set; }

        public LoginUserInfo LoginSeller { get; private set; }

        public LoginUserInfo LoginAdmin { get; private set; }
        #endregion

        public WebWorkContext() : this(System.Web.HttpContext.Current)
        { }

        public WebWorkContext(System.Web.HttpContext context)
        {
            if (context == null)
            {
                throw new Exception("上下文不能为空");
            }
            this.context = context;

            this.IsPost = context.Request.IsPost();
            this.IsAjax = context.Request.IsAjax();
            this.IP = context.Request.Ip();
            this.BaseUrl = context.Request.GetBaseUrl();
            this.Url = context.Request.GetCurrentUrl();
            //login user
            LoadLoginUser();
        }

        public void LoadLoginUser()
        {
            this.SSOUser = AccountHelper.SSO.GetLoginUser(context);
            this.LoginUser = AccountHelper.User.GetLoginUser(context);
            this.LoginTrader = AccountHelper.Trader.GetLoginUser(context);
            this.LoginSeller = AccountHelper.Seller.GetLoginUser(context);
            this.LoginAdmin = AccountHelper.Admin.GetLoginUser(context);
        }

        public void Dispose()
        {
            //
        }
    }
}
