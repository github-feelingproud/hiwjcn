using Hiwjcn.Core.Infrastructure.Common;
using Hiwjcn.Core.Infrastructure.User;
using Lib.helper;
using Lib.ioc;
using Lib.mvc;
using Lib.mvc.user;
using Model.Sys;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace WebCore.MvcLib.Controller
{
    [ValidateInput(false)]
    public class UserBaseController : BaseController
    {
        /// <summary>
        /// 网站配置
        /// </summary>
        public List<OptionModel> Settings { get; private set; }

        public UserBaseController()
        {
            //自动登录还有问题
            //AutoLoginSSO();
            AutoLogin();

            //加载数据库option
            LoadSettings();

            this.X.LoadLoginUser();
        }

        /// <summary>
        /// 不实用sso，直接查数据库
        /// </summary>
        [NonAction]
        private void AutoLogin()
        {
            var loginuser = AccountHelper.User.GetLoginUser();
            if (loginuser != null) { return; }

            var email = AccountHelper.User.GetCookieUID();
            var token = AccountHelper.User.GetCookieToken();
            if (ValidateHelper.IsAllPlumpString(email, token))
            {
                var bll = AppContext.GetObject<IUserService>();
                var model = bll.LoginByToken(email, token);
                //如果通过token登陆成功
                if (model != null && model.UserToken != null)
                {
                    AccountHelper.User.SetUserLogin(loginuser: new LoginUserInfo() { });
                    return;
                }
            }
            AccountHelper.User.SetUserLogout();
        }

        /// <summary>
        /// 加载网站配置项
        /// </summary>
        [NonAction]
        private void LoadSettings()
        {
            //options
            var setting = AppContext.GetObject<ISettingService>();
            setting.UseCache = true;
            this.Settings = setting.GetAllOptions();
            if (this.Settings == null)
            {
                this.Settings = new List<OptionModel>();
            }
            if (this.Settings.Count <= 0) { return; }
            var keys = new string[]
            {
                "web_name", "web_description","web_keywords",
                "web_url",
                "web_email","web_phone","web_address"
            };
            foreach (var key in keys)
            {
                ViewData[key] = this.GetOption(key);
            }
        }

        /// <summary>
        /// 获取网站配置，没有值就返回空字符串
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [NonAction]
        public string GetOption(string key)
        {
            return ConvertHelper.GetString(Settings?.FirstOrDefault(x => x.Key == key)?.Value);
        }

    }
}
