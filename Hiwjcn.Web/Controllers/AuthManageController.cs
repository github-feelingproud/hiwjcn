using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lib.mvc;
using Lib.mvc.auth;
using Lib.mvc.user;
using Lib.helper;
using Lib.core;
using Lib.extension;
using Lib.net;
using System.Threading.Tasks;
using Hiwjcn.Core.Infrastructure.Auth;
using Lib.mvc.auth.validation;
using Hiwjcn.Core.Domain.Auth;
using Lib.data;

namespace Hiwjcn.Web.Controllers
{
    public class AuthManageController : BaseController
    {
        public const string Permission = "manage.auth";

        private readonly IValidationDataProvider _IValidationDataProvider;

        private readonly IAuthLoginService _IAuthLoginService;

        private readonly IAuthTokenService _IAuthTokenService;
        private readonly IAuthScopeService _IAuthScopeService;
        private readonly IAuthClientService _IAuthClientService;

        private readonly IRepository<AuthScope> _AuthScopeRepository;
        private readonly IRepository<AuthClient> _AuthClientRepository;

        public AuthManageController(
            IValidationDataProvider _IValidationDataProvider,
            IAuthLoginService _IAuthLoginService,
            IAuthTokenService _IAuthTokenService,
            IAuthScopeService _IAuthScopeService,
            IAuthClientService _IAuthClientService,
            IRepository<AuthScope> _AuthScopeRepository,
            IRepository<AuthClient> _AuthClientRepository)
        {
            this._IValidationDataProvider = _IValidationDataProvider;
            this._IAuthLoginService = _IAuthLoginService;

            this._IAuthTokenService = _IAuthTokenService;
            this._IAuthScopeService = _IAuthScopeService;
            this._IAuthClientService = _IAuthClientService;

            this._AuthScopeRepository = _AuthScopeRepository;
            this._AuthClientRepository = _AuthClientRepository;
        }

        [PageAuth(Permission = Permission)]
        public async Task<ActionResult> Scopes()
        {
            throw new NotImplementedException();
        }

        [ApiAuth(Permission = Permission)]
        public async Task<ActionResult> SaveScopeAction()
        {
            throw new NotImplementedException();
        }

        [PageAuth(Permission = Permission)]
        public async Task<ActionResult> Clients()
        {
            throw new NotImplementedException();
        }



        public async Task<ActionResult> InitData()
        {
            return await RunActionAsync(async () =>
            {
                var now = DateTime.Now;

                if (!await this._AuthScopeRepository.ExistAsync(null))
                {
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
                            Important=(int)YesOrNoEnum.否,
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
                            Important=(int)YesOrNoEnum.否,
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
                            Important=(int)YesOrNoEnum.否,
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

                if (!await this._AuthClientRepository.ExistAsync(null))
                {
                    var list = new List<AuthClient>()
                    {
                        new AuthClient()
                        {
                            UID=Com.GetUUID(),
                            ClientName="汽配龙IOS客户端",
                            Description="汽配龙IOS客户端",
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
                            ClientName="汽配龙Android客户端",
                            Description="汽配龙Android客户端",
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

                var client_id = this._IValidationDataProvider.GetClientID(this.X.context);
                var client_security = this._IValidationDataProvider.GetClientSecurity(this.X.context);

                if (!ValidateHelper.IsAllPlumpString(client_id, client_security))
                {
                    return Content("default client data is empty");
                }

                if (!await this._AuthClientRepository.ExistAsync(x => x.UID == client_id && x.ClientSecretUID == client_security))
                {
                    await this._AuthClientRepository.DeleteWhereAsync(x => x.UID == client_id || x.ClientSecretUID == client_security);
                    var official = new AuthClient()
                    {
                        UID = client_id,
                        ClientName = "auth管理端",
                        Description = "auth管理端",
                        ClientUrl = "http://images.qipeilong.cn/ico/logo.png?t=111",
                        LogoUrl = "http://images.qipeilong.cn/ico/logo.png?t=111",
                        UserUID = "http://www.baidu.com/",
                        ClientSecretUID = client_security,
                        IsRemove = (int)YesOrNoEnum.否,
                        IsActive = (int)YesOrNoEnum.是,
                        CreateTime = now,
                        UpdateTime = now
                    };
                    await this._AuthClientRepository.AddAsync(official);
                }

                return Content("ok");
            });
        }
    }
}