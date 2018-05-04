using Hiwjcn.Framework;
using Hiwjcn.Service.Epc;
using Hiwjcn.Service.MemberShip;
using Lib.cache;
using Lib.extension;
using Lib.helper;
using Lib.mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Hiwjcn.Web.Areas.Epc.Controllers
{
    /// <summary>
    /// 数据报表
    /// </summary>
    public class DashboardController : EpcBaseController
    {
        private readonly ICacheProvider _cache;
        private readonly IDashboardService _dashService;
        private readonly IMemberShipDashboardService _memberDashService;

        public DashboardController(
            ICacheProvider _cache,
            IDashboardService _dashService,
            IMemberShipDashboardService _memberDashService)
        {
            this._cache = _cache;
            this._dashService = _dashService;
            this._memberDashService = _memberDashService;
        }

        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> MonthlyIssue()
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.ManagerRole);

                var end = DateTime.Now.Date.AddDays(1);
                var start = end.AddMonths(-2);

                var key = "dash.monthly_issue_count".WithCacheKeyPrefix();

                var data = await this._cache.GetOrSetAsync(key,
                    async () => await this._dashService.IssueCountGroupByDay(org_uid, start, end),
                    TimeSpan.FromMinutes(5));
                data = ConvertHelper.NotNullList(data);

                var date = new List<string>();
                var data1 = new List<int>();
                var data2 = new List<int>();

                for (var d = start; d <= end; d = d.AddDays(1))
                {
                    date.Add(d.ToShortDateString());
                    var list = data.Where(x => x.Year == d.Year && x.Month == d.Month && x.Day == d.Day);
                    var open = list.FirstOrDefault(x => x.IsClosed <= 0);
                    var close = list.FirstOrDefault(x => x.IsClosed > 0);
                    data1.Add(open?.Count ?? 0);
                    data2.Add(close?.Count ?? 0);
                }

                return GetJson(new _()
                {
                    success = true,
                    data = new
                    {
                        date,
                        data1,
                        data2
                    }
                });
            });
        }

        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> MonthlyIssueDevice()
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.ManagerRole);

                var end = DateTime.Now.Date.AddDays(1);
                var start = end.AddMonths(-1);

                var key = "dash.monthly_device_issue_count".WithCacheKeyPrefix();

                var data = await this._cache.GetOrSetAsync(key,
                    async () => await this._dashService.IssueCountGroupByDevice(org_uid, start, end),
                    TimeSpan.FromMinutes(5));
                data = ConvertHelper.NotNullList(data);

                data = data.Where(x => x.Count > 0 && x.DeviceModel != null).ToList();

                var device = new List<string>();
                var count = new List<int>();

                var res = data.OrderByDescending(x => x.Count).Select(x => new
                {
                    count = x.Count,
                    device = x.DeviceModel.Name
                }).ToList();

                return GetJson(new _()
                {
                    success = true,
                    data = res
                });
            });
        }

        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> CountBox()
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.ManagerRole);

                var border = DateTime.Now.GetDateBorder();

                var key = "dash.today_issue_count".WithCacheKeyPrefix();

                var issue = await this._cache.GetOrSetAsync(key,
                    async () => await this._dashService.IssueCount(org_uid, border.start, border.end),
                    TimeSpan.FromMinutes(5));

                key = "dash.device_count".WithCacheKeyPrefix();
                var device = await this._cache.GetOrSetAsync(key,
                    async () => await this._dashService.DeviceCount(org_uid),
                    TimeSpan.FromMinutes(5));

                key = "dash.check_count".WithCacheKeyPrefix();
                var check_count = await this._cache.GetOrSetAsync(key,
                    async () => await this._dashService.CheckLogCount(org_uid, border.start, border.end),
                    TimeSpan.FromMinutes(5));

                key = "dash.member_count".WithCacheKeyPrefix();
                var member = await this._cache.GetOrSetAsync(key,
                    async () => await this._memberDashService.MemberCount(org_uid),
                    TimeSpan.FromMinutes(5));

                return GetJson(new _()
                {
                    success = true,
                    data = new
                    {
                        issue,
                        device,
                        check_count,
                        member
                    }
                });
            });
        }

        [HttpPost, EpcAuth]
        public async Task<ActionResult> TopWorkHardWorker()
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();

                var now = DateTime.Now;
                var key = "dash.topworkharduser".WithCacheKeyPrefix();

                var data = await this._cache.GetOrSetAsync(key,
                    async () => await this._dashService.CheckLogGroupByUser(org_uid, now.AddMonths(-3), now, 10),
                    TimeSpan.FromMinutes(10));

                data = ConvertHelper.NotNullList(data);

                return GetJson(new _()
                {
                    success = true,
                    data = data,
                });
            });
        }
    }
}