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
using Hiwjcn.Core.Model.Sys;
using System.Data.Entity;
using Hiwjcn.Framework;
using Lib.cache;
using Lib.task;

namespace Hiwjcn.Web.Controllers
{
    public class AuthManageController : BaseController
    {
        public const string manage_auth = "manage.auth";

        private readonly IValidationDataProvider _IValidationDataProvider;
        private readonly IAuthLoginService _IAuthLoginService;
        private readonly IAuthTokenService _IAuthTokenService;
        private readonly IAuthScopeService _IAuthScopeService;
        private readonly IAuthClientService _IAuthClientService;
        private readonly IRepository<AuthScope> _AuthScopeRepository;
        private readonly IRepository<AuthClient> _AuthClientRepository;
        private readonly IRepository<ReqLogModel> _ReqLogModelRepository;
        private readonly IRepository<CacheHitLog> _CacheHitLogRepository;
        private readonly ICacheProvider _cache;

        public AuthManageController(
            IValidationDataProvider _IValidationDataProvider,
            IAuthLoginService _IAuthLoginService,
            IAuthTokenService _IAuthTokenService,
            IAuthScopeService _IAuthScopeService,
            IAuthClientService _IAuthClientService,
            IRepository<AuthScope> _AuthScopeRepository,
            IRepository<AuthClient> _AuthClientRepository,
            IRepository<ReqLogModel> _ReqLogModelRepository,
            IRepository<CacheHitLog> _CacheHitLogRepository,
            ICacheProvider _cache)
        {
            this._IValidationDataProvider = _IValidationDataProvider;
            this._IAuthLoginService = _IAuthLoginService;
            this._IAuthTokenService = _IAuthTokenService;
            this._IAuthScopeService = _IAuthScopeService;
            this._IAuthClientService = _IAuthClientService;
            this._AuthScopeRepository = _AuthScopeRepository;
            this._AuthClientRepository = _AuthClientRepository;
            this._ReqLogModelRepository = _ReqLogModelRepository;
            this._CacheHitLogRepository = _CacheHitLogRepository;
            this._cache = _cache;
        }

        [PageAuth(Permission = manage_auth)]
        [RequestLog]
        public async Task<ActionResult> Statics()
        {
            return await RunActionAsync(async () =>
            {
                var now = DateTime.Now;
                var count = 10;
                var start = now.AddDays(-count);
                var expire = TimeSpan.FromMinutes(10);

                await this._ReqLogModelRepository.PrepareSessionAsync(async db =>
                {
                    var reqlog_query = db.Set<ReqLogModel>().AsNoTrackingQueryable();
                    var cachehit_query = db.Set<CacheHitLog>().AsNoTrackingQueryable();

                    #region 今天请求频次
                    var border = now.GetDateBorder();
                    var reqlog_groupbyhour = await this._cache.GetOrSetAsync(
                        "auth.statics.reqlog_groupbyhour".WithCacheKeyPrefix(), async () =>
                        {
                            return await reqlog_query
                            .Where(x => x.CreateTime >= border.start && x.CreateTime < border.end && x.IsRemove <= 0)
                            .GroupBy(x => x.TimeHour)
                            .Select(x => new ReqLogGroupModel()
                            {
                                Hour = x.Key,
                                ReqCount = x.Count()
                            }).ToListAsync();
                        }, expire);

                    if (reqlog_groupbyhour != null)
                    {
                        for (var i = 0; i <= now.Hour; ++i)
                        {
                            var hour_data = reqlog_groupbyhour.Where(x => x.Hour == i).FirstOrDefault();
                            if (hour_data == null)
                            {
                                reqlog_groupbyhour.Add(new ReqLogGroupModel()
                                {
                                    Hour = i,
                                    ReqCount = 0
                                });
                            }
                        }
                        reqlog_groupbyhour = reqlog_groupbyhour.OrderBy(x => x.Hour).ToList();
                    }

                    ViewData[nameof(reqlog_groupbyhour)] = reqlog_groupbyhour;
                    #endregion

                    #region 请求日志
                    //请求日志按照时间分组
                    var reqlog_groupbytime = await this._cache.GetOrSetAsync(
                        "auth.statics.reqlog_groupbytime".WithCacheKeyPrefix(), async () =>
                     {
                         return await reqlog_query
                         .Where(x => x.CreateTime >= start && x.IsRemove <= 0)
                         .GroupBy(x => new { x.TimeYear, x.TimeMonth, x.TimeDay })
                         .Select(x => new ReqLogGroupModel()
                         {
                             Year = x.Key.TimeYear,
                             Month = x.Key.TimeMonth,
                             Day = x.Key.TimeDay,
                             ReqTime = x.Average(m => m.ReqTime),
                             ReqCount = x.Count()
                         }).Take(count).ToListAsync();
                     }, expire);

                    ViewData[nameof(reqlog_groupbytime)] = reqlog_groupbytime;
                    //请求日志按照控制器分组
                    var reqlog_groupbyaction = await this._cache.GetOrSetAsync(
                        "auth.statics.reqlog_groupbyaction".WithCacheKeyPrefix(), async () =>
                     {
                         return await reqlog_query
                         .Where(x => x.CreateTime >= start && x.IsRemove <= 0)
                         .GroupBy(x => new { x.AreaName, x.ControllerName, x.ActionName })
                         .Select(x => new ReqLogGroupModel()
                         {
                             AreaName = x.Key.AreaName,
                             ControllerName = x.Key.ControllerName,
                             ActionName = x.Key.ActionName,
                             ReqTime = x.Average(m => m.ReqTime),
                             ReqCount = x.Count()
                         }).Take(count).ToListAsync();
                     }, expire);

                    ViewData[nameof(reqlog_groupbyaction)] = reqlog_groupbyaction;
                    #endregion

                    #region 缓存命中
                    //缓存命中按照时间分组
                    var cachehit_groupbytime = await this._cache.GetOrSetAsync(
                        "auth.statics.cachehit_groupbytime".WithCacheKeyPrefix(), async () =>
                     {
                         return await cachehit_query
                         .Where(x => x.CreateTime >= start && x.IsRemove <= 0)
                         .GroupBy(x => new { x.TimeYear, x.TimeMonth, x.TimeDay })
                         .Select(x => new CacheHitGroupModel()
                         {
                             Year = x.Key.TimeYear,
                             Month = x.Key.TimeMonth,
                             Day = x.Key.TimeDay,
                             HitCount = x.Sum(m => m.Hit),
                             NotHitCount = x.Sum(m => m.NotHit)
                         }).Take(count).ToListAsync();
                     }, expire);

                    ViewData[nameof(cachehit_groupbytime)] = cachehit_groupbytime;
                    //缓存命中按照key分组
                    var cachehit_groupbykey = await this._cache.GetOrSetAsync(
                        "auth.statics.cachehit_groupbykey".WithCacheKeyPrefix(), async () =>
                    {
                        return await cachehit_query
                        .Where(x => x.CreateTime >= start && x.IsRemove <= 0)
                        .GroupBy(x => x.CacheKey).Select(x => new CacheHitGroupModel()
                        {
                            CacheKey = x.Key,
                            HitCount = x.Sum(m => m.Hit),
                            NotHitCount = x.Sum(m => m.NotHit)
                        }).Take(count).ToListAsync();
                    }, expire);

                    ViewData[nameof(cachehit_groupbykey)] = cachehit_groupbykey;
                    #endregion
                    return true;
                });
                return View();
            });
        }

        [PageAuth(Permission = manage_auth)]
        [RequestLog]
        public async Task<ActionResult> Scopes(string q, int? page)
        {
            return await RunActionAsync(async () =>
            {
                page = CheckPage(page);
                var pagesize = 20;

                var data = await this._IAuthScopeService.QueryPager(q, page.Value, pagesize);

                ViewData["list"] = data.DataList;
                ViewData["pager"] = data.GetPagerHtml(this.RouteData.ActionUrl(), nameof(page), page.Value, pagesize, this.X.context);

                return View();
            });
        }

        [PageAuth(Permission = manage_auth)]
        [RequestLog]
        [Route("AuthManage/EditScope/{uid}")]
        public async Task<ActionResult> EditScope(string uid)
        {
            return await RunActionAsync(async () =>
            {
                var model = await this._AuthScopeRepository.GetFirstAsync(x => x.UID == uid);

                ViewData["model"] = model;
                return View();
            });
        }

        [ApiAuth(Permission = manage_auth)]
        [RequestLog]
        public Task<ActionResult> SaveScopeAction()
        {
            throw new NotImplementedException();
        }

        [PageAuth(Permission = manage_auth)]
        [RequestLog]
        public async Task<ActionResult> Clients(string q, int? page)
        {
            return await RunActionAsync(async () =>
            {
                page = CheckPage(page);
                var pagesize = 20;

                var data = await this._IAuthClientService.QueryListAsync(q: q, page: page.Value, pagesize: pagesize);

                ViewData["list"] = data.DataList;
                ViewData["pager"] = data.GetPagerHtml(this.RouteData.ActionUrl(), nameof(page), page.Value, pagesize, this.X.context);

                return View();
            });
        }

        [PageAuth(Permission = manage_auth)]
        [RequestLog]
        public async Task<ActionResult> Tasks()
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);

                try
                {
                    ViewData["list"] = TaskManager.GetAllTasks();
                }
                catch (Exception e)
                {
                    e.AddErrorLog();
                }

                return View();
            });
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