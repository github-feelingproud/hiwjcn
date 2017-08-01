using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lib.mvc;
using Lib.mvc.user;
using Lib.helper;
using Lib.core;
using Lib.data;
using Lib.extension;
using Lib.net;
using Lib.cache;
using System.Threading.Tasks;
using Hiwjcn.Core.Infrastructure.Auth;
using Hiwjcn.Bll.Auth;
using Hiwjcn.Core.Domain.Auth;
using Lib.mvc.auth;
using Hiwjcn.Framework;
using System.Collections.ObjectModel;

namespace Hiwjcn.Web.Controllers
{
    [RoutePrefix("connect")]
    public class ConnectController : BaseController
    {
        private readonly IAuthLoginService _IAuthLoginService;

        private readonly IAuthTokenService _IAuthTokenService;
        private readonly IAuthScopeService _IAuthScopeService;
        private readonly IAuthClientService _IAuthClientService;

        private readonly IRepository<AuthScope> _AuthScopeRepository;
        private readonly IRepository<AuthClient> _AuthClientRepository;

        public ConnectController(
            IAuthLoginService _IAuthLoginService,
            IAuthTokenService _IAuthTokenService,
            IAuthScopeService _IAuthScopeService,
            IAuthClientService _IAuthClientService,
            IRepository<AuthScope> _AuthScopeRepository,
            IRepository<AuthClient> _AuthClientRepository)
        {
            this._IAuthLoginService = _IAuthLoginService;

            this._IAuthTokenService = _IAuthTokenService;
            this._IAuthScopeService = _IAuthScopeService;
            this._IAuthClientService = _IAuthClientService;

            this._AuthScopeRepository = _AuthScopeRepository;
            this._AuthClientRepository = _AuthClientRepository;
        }

        private readonly ReadOnlyDictionary<string, string> LoginTypeDict = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()
        {
            ["password"] = "~/Views/Connect/Login.cshtml",
            ["onetimecode"] = "~/Views/Connect/LoginByCode.cshtml",
            //["token"] = "",
        });

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> LoginByPassword(string username, string password)
        {
            return await RunActionAsync(async () =>
            {
                var data = await this._IAuthLoginService.LoginByPassword(username, password);
                if (data.success)
                {
                    this.X.context.CookieLogin(data.data);
                    return GetJsonRes(string.Empty);
                }
                return GetJsonRes(data.msg);
            });
        }

        [HttpPost]
        [RequestLog]
        public async Task<ActionResult> LoginByOneTimeCode(string phone, string code)
        {
            return await RunActionAsync(async () =>
            {
                var data = await this._IAuthLoginService.LoginByCode(phone, code);

                if (ValidateHelper.IsPlumpString(data.msg))
                {
                    return GetJsonRes(data.msg);
                }

                this.X.context.CookieLogin(data.data);

                return GetJsonRes(string.Empty);
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

        [RequestLog]
        public async Task<ActionResult> Authorize(string client_id, string redirect_uri, string scope,
            string response_type, string state, string login_type, string return_type)
        {
            return await RunActionAsync(async () =>
            {
                if (!ValidateHelper.IsPlumpString(redirect_uri))
                {
                    return Content($"{nameof(redirect_uri)}为空");
                }

                ViewData[nameof(redirect_uri)] = redirect_uri;
                ViewData[nameof(login_type)] = login_type;
                ViewData[nameof(return_type)] = return_type;

                var scopes = ConvertHelper.GetString(scope).Trim().Split(',').Where(x => ValidateHelper.IsPlumpString(x)).ToList();
                var scopelist = await this._IAuthScopeService.GetScopesOrDefault(scopes.ToArray());
                ViewData["scopes"] = scopelist.OrderByDescending(x => x.Important).ToList();

                var client = await this._IAuthClientService.GetByID(client_id);
                if (client == null)
                {
                    return Content("client_id无效");
                }
                ViewData["client"] = client;

                //使用异步加载
                var loginuser = await this.X.context.GetAuthUserAsync();

                return View();
            });
        }

        [HttpPost]
        [ApiAuth]
        [RequestLog]
        public async Task<ActionResult> CreateAuthorizeCode(string client_id, string scope)
        {
            return await RunActionAsync(async () =>
            {
                var loginuser = await this.X.context.GetAuthUserAsync();

                var data = await this._IAuthTokenService.CreateCodeAsync(client_id, scope?.JsonToEntity<List<string>>(), loginuser.UserID);
                if (ValidateHelper.IsPlumpString(data.msg))
                {
                    return GetJsonRes(data.msg);
                }
                return GetJson(new _() { success = true, data = data.code?.UID });
            });
        }

        public ActionResult Logout(string url)
        {
            return RunAction(() =>
            {
                this.X.context.CookieLogout();

                if (!ValidateHelper.IsPlumpString(url))
                {
                    url = "/";
                }

                return Redirect(url);
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


        [PageAuth]
        [RequestLog]
        public async Task<ActionResult> MyClients(string q, int? page)
        {
            return await RunActionAsync(async () =>
            {
                page = CheckPage(page);
                var pagesize = 100;

                var loginuser = await this.X.context.GetAuthUserAsync();

                var data = await this._IAuthTokenService.GetMyAuthorizedClientsAsync(loginuser?.UserID, q, page.Value, pagesize);
                ViewData["pager"] = data.GetPagerHtml($"Auth/{nameof(MyClients)}", "page", page.Value, pagesize);
                ViewData["list"] = data.DataList;
                return View();
            });
        }

        public async Task<ActionResult> InitScopes()
        {
            return await RunActionAsync(async () =>
            {
                if (!await this._AuthScopeRepository.ExistAsync(null))
                {
                    var now = DateTime.Now;
                    var list = new List<AuthScope>()
                    {
                        new AuthScope()
                        {
                            UID=Com.GetUUID(),
                            Name="order",
                            DisplayName="订单",
                            Description="订单",
                            Important=(int)YesOrNoEnum.是,
                            Sort=0,
                            IsDefault=(int)YesOrNoEnum.是,
                            ImageUrl="http://www.baidu.com/logo.png",
                            FontIcon="",
                            CreateTime=now,
                            UpdateTime=now
                        },
                        new AuthScope()
                        {
                            UID=Com.GetUUID(),
                            Name="product",
                            DisplayName="商品",
                            Description="商品",
                            Important=(int)YesOrNoEnum.是,
                            Sort=0,
                            IsDefault=(int)YesOrNoEnum.是,
                            ImageUrl="http://www.baidu.com/logo.png",
                            FontIcon="",
                            CreateTime=now,
                            UpdateTime=now
                        },
                        new AuthScope()
                        {
                            UID=Com.GetUUID(),
                            Name="user",
                            DisplayName="个人信息",
                            Description="个人信息",
                            Important=(int)YesOrNoEnum.是,
                            Sort=0,
                            IsDefault=(int)YesOrNoEnum.是,
                            ImageUrl="http://www.baidu.com/logo.png",
                            FontIcon="",
                            CreateTime=now,
                            UpdateTime=now
                        },
                        new AuthScope()
                        {
                            UID=Com.GetUUID(),
                            Name="auth",
                            DisplayName="auth",
                            Description="auth",
                            Important=(int)YesOrNoEnum.是,
                            Sort=0,
                            IsDefault=(int)YesOrNoEnum.是,
                            ImageUrl="http://www.baidu.com/logo.png",
                            FontIcon="",
                            CreateTime=now,
                            UpdateTime=now
                        },
                        new AuthScope()
                        {
                            UID=Com.GetUUID(),
                            Name="inquiry",
                            DisplayName="询价单",
                            Description="询价单",
                            Important=(int)YesOrNoEnum.是,
                            Sort=0,
                            IsDefault=(int)YesOrNoEnum.是,
                            ImageUrl="http://www.baidu.com/logo.png",
                            FontIcon="",
                            CreateTime=now,
                            UpdateTime=now
                        }
                    };

                    await this._AuthScopeRepository.AddAsync(list.ToArray());
                }
                return Content("ok");
            });
        }

        public async Task<ActionResult> InitClients()
        {
            return await RunActionAsync(async () =>
            {
                if (!await this._AuthClientRepository.ExistAsync(null))
                {
                    var now = DateTime.Now;
                    var list = new List<AuthClient>()
                    {
                        new AuthClient()
                        {
                            UID=Com.GetUUID(),
                            ClientName="IOS",
                            ClientUrl="http://images.qipeilong.cn/ico/logo.png?t=111",
                            LogoUrl="http://images.qipeilong.cn/ico/logo.png?t=111",
                            UserUID="http://www.baidu.com/",
                            ClientSecretUID=Com.GetUUID(),
                            IsRemove=(int)YesOrNoEnum.否,
                            IsActive=(int)YesOrNoEnum.是,
                            CreateTime=now,
                            UpdateTime=now
                        },
                        new AuthClient()
                        {
                            UID=Com.GetUUID(),
                            ClientName="Android",
                            ClientUrl="http://images.qipeilong.cn/ico/logo.png?t=111",
                            LogoUrl="http://images.qipeilong.cn/ico/logo.png?t=111",
                            UserUID="http://www.baidu.com/",
                            ClientSecretUID=Com.GetUUID(),
                            IsRemove=(int)YesOrNoEnum.否,
                            IsActive=(int)YesOrNoEnum.是,
                            CreateTime=now,
                            UpdateTime=now
                        },
                    };

                    await this._AuthClientRepository.AddAsync(list.ToArray());
                }
                return Content("ok");
            });
        }
    }
}