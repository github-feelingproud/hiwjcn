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
using System.Linq;
using WebLogic.Model.User;
using Lib.data;
using Lib.extension;
using Hiwjcn.Core.Domain.Auth;
using Lib.mvc.auth;
using Lib.cache;
using WebCore.MvcLib.Controller;
using Hiwjcn.Core.Infrastructure.Auth;
using Lib.mvc.auth.validation;

namespace Hiwjcn.Web.Controllers
{
    public class AccountController : UserBaseController
    {
        private readonly IAuthLoginService _IAuthLoginService;
        private readonly IUserService _IUserService;
        private readonly ILoginLogService _LoginErrorLogBll;
        private readonly LoginStatus _LoginStatus;
        private readonly IRepository<AuthScope> _AuthScopeRepository;
        private readonly IAuthTokenService _IAuthTokenService;
        private readonly IValidationDataProvider _IValidationDataProvider;

        public AccountController(
            IAuthLoginService _IAuthLoginService,
            IUserService user,
            ILoginLogService loginlog,
            LoginStatus logincontext,
            IRepository<AuthScope> _AuthScopeRepository,
            IAuthTokenService _IAuthTokenService,
            IValidationDataProvider _IValidationDataProvider)
        {
            this._IAuthLoginService = _IAuthLoginService;
            this._IUserService = user;
            this._LoginErrorLogBll = loginlog;
            this._LoginStatus = logincontext;
            this._AuthScopeRepository = _AuthScopeRepository;
            this._IAuthTokenService = _IAuthTokenService;
            this._IValidationDataProvider = _IValidationDataProvider;
        }

        #region 登录

        [NonAction]
        private async Task<string> LogLoginErrorInfo(string user_name, string password, Func<Task<string>> func)
        {
            if (!ValidateHelper.IsAllPlumpString(user_name, password))
            {
                return "登录信息未填写";
            }
            if (await this._LoginErrorLogBll.GetRecentLoginErrorTimes(user_name) > 5)
            {
                return "你短时间内有多次错误登录记录，请稍后再试";
            }
            var res = await func.Invoke();
            if (ValidateHelper.IsPlumpString(res))
            {
                var errinfo = new LoginErrorLogModel()
                {
                    LoginKey = user_name,
                    LoginPwd = password,
                    LoginIP = this.X.IP,
                    ErrorMsg = res
                };
                var logres = await this._LoginErrorLogBll.AddLoginErrorLog(errinfo);
                if (ValidateHelper.IsPlumpString(logres))
                {
                    new Exception($"记录错误登录日志错误:{logres}").AddErrorLog();
                }
            }
            return res;
        }

        [NonAction]
        private async Task<_<LoginUserInfo>> CreateAuthToken(LoginUserInfo loginuser)
        {
            var data = new _<LoginUserInfo>();
            if (loginuser == null)
            {
                data.SetErrorMsg("登录失败");
                return data;
            }

            var client_id = this._IValidationDataProvider.GetClientID(this.X.context);
            var client_security = this._IValidationDataProvider.GetClientSecurity(this.X.context);

            var allscopes = await this._AuthScopeRepository.GetListAsync(null);
            var code = await this._IAuthTokenService.CreateCodeAsync(client_id, allscopes.Select(x => x.Name).ToList(), loginuser.UserID);

            if (ValidateHelper.IsPlumpString(code.msg))
            {
                data.SetErrorMsg(code.msg);
                return data;
            }

            var token = await this._IAuthTokenService.CreateTokenAsync(client_id, client_security, code.data.UID);
            if (ValidateHelper.IsPlumpString(token.msg))
            {
                data.SetErrorMsg(token.msg);
                return data;
            }

            loginuser.LoginToken = token.data.UID;
            loginuser.RefreshToken = token.data.RefreshToken;
            loginuser.TokenExpire = token.data.ExpiryTime;

            data.SetSuccessData(loginuser);
            return data;
        }

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> LoginByPassword(string username, string password)
        {
            return await RunActionAsync(async () =>
            {
                var res = await this.LogLoginErrorInfo(username, password, async () =>
                {
                    var data = await this._IAuthLoginService.LoginByPassword(username, password);
                    if (ValidateHelper.IsPlumpString(data.msg))
                    {
                        return data.msg;
                    }
                    var loginuser = await this.CreateAuthToken(data.data);
                    if (ValidateHelper.IsPlumpString(loginuser.msg))
                    {
                        return loginuser.msg;
                    }
                    this.X.context.CookieLogin(loginuser.data);
                    return string.Empty;
                });
                return GetJsonRes(res);
            });
        }

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> LoginByOneTimeCode(string phone, string code)
        {
            return await RunActionAsync(async () =>
            {
                var res = await this.LogLoginErrorInfo(phone, code, async () =>
                {
                    var data = await this._IAuthLoginService.LoginByCode(phone, code);
                    if (ValidateHelper.IsPlumpString(data.msg))
                    {
                        return data.msg;
                    }
                    var loginuser = await this.CreateAuthToken(data.data);
                    if (ValidateHelper.IsPlumpString(loginuser.msg))
                    {
                        return loginuser.msg;
                    }
                    this.X.context.CookieLogin(loginuser.data);
                    return string.Empty;
                });
                return GetJsonRes(res);
            });
        }

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> SendOneTimeCode(string phone)
        {
            return await RunActionAsync(async () =>
            {
                var data = await this._IAuthLoginService.SendOneTimeCode(phone);
                return GetJsonRes(data);
            });
        }

        /// <summary>
        /// 登录账户
        /// </summary>
        /// <returns></returns>
        [RequestLog]
        public async Task<ActionResult> Login(string url, string @continue, string next, string callback)
        {
            return await RunActionAsync(async () =>
            {
                url = Com.FirstPlumpStrOrNot(url, @continue, next, callback);
                //url = url ?? @continue ?? next ?? callback;

                var auth_user = await this.X.context.GetAuthUserAsync();
                if (auth_user != null)
                {
                    this._LoginStatus.SetUserLogin(this.X.context, auth_user);
                    if (ValidateHelper.IsPlumpString(url))
                    {
                        return Redirect(url);
                    }
                    return GoHome();
                }

                return View();
            });
        }

        /// <summary>
        /// 退出地址
        /// </summary>
        /// <returns></returns>
        [RequestLog]
        public ActionResult LogOut(string url, string @continue, string next, string callback)
        {
            return RunAction(() =>
            {
                _LoginStatus.SetUserLogout();

                url = Com.FirstPlumpStrOrNot(url, @continue, next, callback);

                if (ValidateHelper.IsPlumpString(url))
                {
                    return Redirect(url);
                }

                return GoHome();
            });
        }

        [RequestLog]
        public async Task<ActionResult> LoginUser()
        {
            return await RunActionAsync(async () =>
            {
                var loginuser = await this.X.context.GetAuthUserAsync();

                return GetJsonp(new _() { success = loginuser != null, data = loginuser });
            });
        }

        #endregion

        #region 注册

        /// <summary>
        /// 注册账户
        /// </summary>
        /// <returns></returns>
        public ActionResult Register()
        {
            return RunAction(() =>
            {
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
        #endregion

        #region 忘记密码

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
    }
}
