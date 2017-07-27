using Hiwjcn.Core.Infrastructure.User;
using Hiwjcn.Framework;
using Lib.core;
using Lib.helper;
using Lib.ioc;
using Lib.mvc;
using Lib.mvc.user;
using Model.User;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebLogic.Bll.User;
using WebLogic.Model.User;

namespace Hiwjcn.Web.Controllers
{
    public class AccountController : WebCore.MvcLib.Controller.UserBaseController
    {
        private bool UseSSO = false;

        private IUserService _IUserService { get; set; }

        private ILoginLogService _LoginErrorLogBll { get; set; }

        private LoginStatus _LoginStatus { get; set; }

        public AccountController(
            IUserService user,
            ILoginLogService loginlog,
            LoginStatus logincontext)
        {
            this._IUserService = user;
            this._LoginErrorLogBll = loginlog;
            this._LoginStatus = logincontext;
        }

        /// <summary>
        /// 获取登录信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetLoginInfo()
        {
            return RunAction(() =>
            {
                var loginuser = this.X.LoginUser;
                return GetJson(new { success = loginuser != null, data = loginuser });
            });
        }

        #region SSO
        /// <summary>
        /// 处理登录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LoginAction(string email, string pass)
        {
            return RunAction(() =>
            {
                if (!ValidateHelper.IsAllPlumpString(email, pass))
                {
                    return GetJsonRes("账户密码不能为空");
                }

                //检查登录尝试
                var err_count = _LoginErrorLogBll.GetRecentLoginErrorTimes(email);
                if (err_count > 5)
                {
                    return GetJsonRes($"你短时间内有{err_count}次错误登录记录，请稍后再试");
                }
                //开始登录
                string msg = string.Empty;
                var model = _IUserService.LoginByPassWord(email, pass, ref msg);
                if (model != null && model.UserToken?.Length > 0)
                {
                    //记录登录状态
                    this._LoginStatus.SetUserLogin(loginuser: new LoginUserInfo() { });
                    return GetJson(new { success = true, msg = "登陆成功" });
                }
                //登录错误，记录错误记录
                var errorLoginLog = new LoginErrorLogModel()
                {
                    LoginKey = ConvertHelper.GetString(email),
                    LoginPwd = ConvertHelper.GetString(pass),
                    LoginIP = ConvertHelper.GetString(this.X.IP),
                };
                var errorloghandler = _LoginErrorLogBll.AddLoginErrorLog(errorLoginLog);
                if (ValidateHelper.IsPlumpString(errorloghandler))
                {
                    LogHelper.Info(this.GetType(), "记录错误登录日志错误：" + errorloghandler);
                }

                return GetJson(new { success = false, msg = msg });
            });
        }

        /// <summary>
        /// 子站post请求这个接口验证token是否正确，并获取返回的loginuser对象
        /// </summary>
        /// <param name="url"></param>
        /// <param name="email"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckChildSiteToken(string email, string token)
        {
            return RunAction(() =>
            {
                if (!ValidateHelper.IsAllPlumpString(email, token))
                {
                    return GetJson(new CheckLoginInfoData() { success = false, message = "参数错误" });
                }
                var usermodel = _IUserService.LoginByToken(email, token);
                if (usermodel == null || usermodel.UserToken == null)
                {
                    return GetJson(new CheckLoginInfoData() { success = false, message = "登陆错误" });
                }
                var info = new LoginUserInfo() { };
                return GetJson(new CheckLoginInfoData() { success = true, data = info });
            });
        }

        /// <summary>
        /// post请求子站回调页面，请求子站主动验证token是否正确
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public ActionResult PostChildCallBack(string url, string callback)
        {
            //这里不要用actionhelper，那么跳转逻辑会复杂
            var loginuser = this.X.LoginUser;

            if (loginuser == null) { return GoHome(); }

            //没有子站信息就跳转默认页
            if (!ValidateHelper.IsAllPlumpString(url, callback))
            {
                return GoHome();
            }

            ViewData["token"] = loginuser.LoginToken;
            ViewData["uid"] = loginuser.UserID;
            ViewData["url"] = url;
            ViewData["callback"] = callback;
            return View();
        }

        [NonAction]
        private ActionResult GoToPostChildCallBack(string url, string callback)
        {
            if (!UseSSO)
            {
                if (ValidateHelper.IsPlumpString(url))
                {
                    return Redirect(url);
                }
                else
                {
                    return GoHome();
                }
            }
            url = ConvertHelper.GetString(url);
            callback = ConvertHelper.GetString(callback);
            return Redirect(string.Format("/Account/PostChildCallBack/?url={0}&callback={1}",
                                       Server.UrlEncode(url), Server.UrlEncode(callback)));
        }

        /// <summary>
        /// 登录账户
        /// </summary>
        /// <returns></returns>
        public ActionResult Login(string url, string callback)
        {
            return RunAction(() =>
            {
                //如果已经登陆
                if (this.X.LoginUser != null)
                {
                    return GoToPostChildCallBack(url, callback);
                }

                var email = _LoginStatus.GetCookieUID();
                var token = _LoginStatus.GetCookieToken();
                if (ValidateHelper.IsAllPlumpString(email, token))
                {
                    var model = _IUserService.LoginByToken(email, token);
                    //如果通过token登陆成功
                    if (model != null && model.UserToken != null)
                    {
                        _LoginStatus.SetUserLogin(loginuser: new LoginUserInfo() { });
                        return GoToPostChildCallBack(url, callback);
                    }
                    _LoginStatus.SetUserLogout();
                }
                return View();
            });
        }

        /// <summary>
        /// 退出地址
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOutSSO()
        {
            return RunAction(() =>
            {
                _LoginStatus.SetUserLogout();
                return View();
            });
        }

        /// <summary>
        /// 注册账户
        /// </summary>
        /// <returns></returns>
        public ActionResult Register()
        {
            return RunAction(() =>
            {
                if (this.GetOption("can_reg").ToLower() != "true")
                {
                    return Content("管理员关闭了注册功能");
                }
                return View();
            });
        }

        /// <summary>
        /// 处理注册
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [RequestLog]
        public ActionResult RegisterAction(string email, string pass, string repass, string verify)
        {
            return RunAction(() =>
            {
                if (this.GetOption("can_reg").ToLower() != "true")
                {
                    return GetJsonRes("管理员关闭了注册功能");
                }
                if (!ValidateHelper.IsAllPlumpString(email, pass, repass, verify))
                {
                    return GetJsonRes("请输入所需内容");
                }
                if (!ValidateHelper.IsLenInRange(email, 5, 30)) { return GetJsonRes("邮箱长度错误"); }
                if (!ValidateHelper.IsLenInRange(pass, 5, 20)) { return GetJsonRes("密码长度错误"); }
                if (verify.Length != 4) { return GetJsonRes("验证码长度必须是4"); }
                if (pass != repass)
                {
                    return GetJsonRes("两次输入密码不一样");
                }
                if (verify != SessionHelper.PopSession<string>(this.X.context.Session, "reg_verify"))
                {
                    return GetJsonRes("验证码错误");
                }

                var model = new UserModel();
                model.NickName = "New User";
                model.Email = email;
                model.PassWord = SecureHelper.GetMD5(pass);
                model.UserImg = "/static/image/moren.png";
                model.Sex = (int)SexEnum.未知;
                model.Flag = (int)(FunctionsEnum.普通用户 | FunctionsEnum.购物 | FunctionsEnum.发帖);

                var res = _IUserService.Register(model);
                return GetJsonRes(res);
            });
        }

        /// <summary>
        /// 获取临时登陆权限
        /// </summary>
        /// <returns></returns>
        public ActionResult Forgot()
        {
            return RunAction(() =>
            {
                if (this.X.IsPostAjax)
                {
                    return Content("未能捕获的请求");
                }
                return View();
            });
        }

        public ActionResult ForgotAction()
        {
            return RunAction(() =>
            {
                return View();
            });
        }
        #endregion

        /// <summary>
        /// 退出地址
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOut()
        {
            return RunAction(() =>
            {
                _LoginStatus.SetUserLogout();
                return GoHome();
            });
        }

        /// <summary>
        /// 单点登录回调
        /// </summary>
        /// <param name="url"></param>
        /// <param name="email"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> GetCallBackResult(string url, string email, string token)
        {
            return await RunActionAsync(async () =>
            {
                if (!ValidateHelper.IsAllPlumpString(email, token))
                {
                    return GoHome();
                }

                var logininfo = await CheckTokenAndSaveLoginStatus(email, token);
                if (ValidateHelper.IsPlumpString(logininfo))
                {
                    return Content(logininfo);
                }
                else
                {
                    //默认跳转地址
                    string deft_url = this.X.BaseUrl + ConfigHelper.Instance.DefaultRedirectUrl;
                    if (!ValidateHelper.IsAllPlumpString(url)) { url = deft_url; }
                    return Redirect(url);
                }
            });
        }


        /// <summary>
        /// 检查token并保存登陆状态
        /// </summary>
        /// <param name="email"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [NonAction]
        private async Task<string> CheckTokenAndSaveLoginStatus(string email, string token)
        {
            var data = await SSOClientHelper.GetCheckTokenResult(email, token);

            if (data != null && data.success && data.data != null)
            {
                //如果当前已经登陆就先退出
                if (this.X.LoginUser != null) { _LoginStatus.SetUserLogout(); }
                //记录登陆状态并跳转
                _LoginStatus.SetUserLogin(loginuser: data.data);
                return string.Empty;
            }
            else
            {
                string msg = string.Empty;
                if (data != null) { msg = data.message; }
                return "获取登陆状态失败,sso服务器返回消息：" + msg;
            }
        }
    }
}
