using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lib.mvc;
using Lib.mvc.user;
using Lib.helper;
using Lib.core;
using Lib.extension;
using Lib.net;
using Lib.data;
using Lib.cache;
using System.Threading.Tasks;
using Hiwjcn.Core.Infrastructure.Auth;
using Hiwjcn.Bll.Auth;
using Hiwjcn.Core.Domain.Auth;
using Lib.events;
using Hiwjcn.Core.Model.Sys;
using Hiwjcn.Framework.Actors;

namespace Hiwjcn.Web.Controllers
{
    [RoutePrefix("oauth2")]
    public class AuthController : BaseController
    {
        private readonly LoginStatus _LoginStatus;
        private readonly IAuthTokenService _IAuthTokenService;
        private readonly IAuthScopeService _IAuthScopeService;
        private readonly ICacheProvider _cache;

        private readonly IRepository<AuthScope> _AuthScopeRepository;
        private readonly IRepository<AuthClient> _AuthClientRepository;

        public AuthController(
            LoginStatus _LoginStatus,
            ICacheProvider _cache,
            IAuthTokenService _IAuthTokenService,
            IAuthScopeService _IAuthScopeService,
            IRepository<AuthScope> _AuthScopeRepository,
            IRepository<AuthClient> _AuthClientRepository)
        {
            this._LoginStatus = _LoginStatus;
            this._cache = _cache;

            this._IAuthTokenService = _IAuthTokenService;
            this._IAuthScopeService = _IAuthScopeService;

            this._AuthScopeRepository = _AuthScopeRepository;
            this._AuthClientRepository = _AuthClientRepository;
        }

        /// <summary>
        /// 用code换token
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="client_secret"></param>
        /// <param name="code"></param>
        /// <param name="grant_type"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("access_token")]
        public async Task<ActionResult> AccessToken(
            string client_id, string client_secret, string code, string grant_type)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return View();
            });
        }

        /// <summary>
        /// 记录1%4的log
        /// </summary>
        private static readonly List<bool> HitLogProbability = new List<bool>() { true, false, false, false };
        private static readonly Random ran = new Random((int)DateTime.Now.Ticks);

        /// <summary>
        /// 合法则返回200， 否则返回404
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [Route("check_token/{client_id}/{access_token}")]
        public async Task<ActionResult> CheckToken(string client_id, string access_token)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                var bad = true;
                if (bad)
                {
                    return Http404();
                }

                var hit_status = CacheHitStatusEnum.Hit;

                var cache_key = AuthCacheKeyManager.TokenKey(access_token);

                var res = await this._cache.GetOrSetAsync(
                    cache_key,
                    async () =>
                    {

                        hit_status = CacheHitStatusEnum.NotHit;
                        return await Task.FromResult("");
                    },
                    TimeSpan.FromMinutes(5));

                if (ran.Choice(HitLogProbability))
                {
                    AkkaHelper<CacheHitLogActor>.Tell(new CacheHitLog()
                    {
                        CacheKey = cache_key,
                        Hit = (int)hit_status
                    });
                }

                return GetJson(new _() { success = true });
            });
        }

        /// <summary>
        /// 检查token是否对某个scope有访问权限
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [Route("check_token_scope/{access_token}/{scope}")]
        public async Task<ActionResult> CheckTokenScope(string access_token, string scope)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                var bad = true;
                if (bad)
                {
                    return Http404();
                }

                return GetJson(new _() { success = true });
            });
        }

        public async Task<ActionResult> MyClients(string q, int? page)
        {
            return await RunActionAsync(async () =>
            {
                page = CheckPage(page);
                var pagesize = 100;

                var data = await this._IAuthTokenService.GetMyAuthorizedClientsAsync(this.X.User?.UserID, q, page.Value, pagesize);
                ViewData["pager"] = data.GetPagerHtml($"oauth/{nameof(MyClients)}", "page", page.Value, pagesize);
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
                            ClientUrl="http://www.baidu.com/",
                            LogoUrl="http://www.baidu.com/",
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
                            ClientUrl="http://www.baidu.com/",
                            LogoUrl="http://www.baidu.com/",
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