using EPC.Core.Entity;
using Hiwjcn.Framework;
using Hiwjcn.Service.Epc;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.DataTypes;
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
    /// job plan
    /// 工作计划，日历显示
    /// </summary>
    public class CalendarController : EpcBaseController
    {
        private readonly ICalendarService _calService;
        private readonly ICheckLogService _checklogService;

        public CalendarController(
            ICalendarService _calService,
            ICheckLogService _checklogService)
        {
            this._calService = _calService;
            this._checklogService = _checklogService;
        }

        /// <summary>
        /// 显示企业所有日历规则
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> QueryRules()
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.AnyRole);
                var data = await this._calService.GetAllEventRule(org_uid);

                return GetJson(new _()
                {
                    success = true,
                    data = data
                });
            });
        }

        /// <summary>
        /// 显示日历
        /// 查询日期范围内的（维护）事件
        /// 翟瑞
        /// </summary>
        /// <param name="org_uid"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> Query(DateTime? start, DateTime? end)
        {
            return await RunActionAsync(async () =>
            {
                var border = DateTime.Now.GetMonthBorder();
                start = start ?? border.start;
                end = end ?? border.end;

                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.AnyRole);

                var res = await this._calService.QueryEvents(org_uid, start.Value, end.Value);

                var device_uids = res.Select(x => x.DeviceUID).Where(x => ValidateHelper.IsPlumpString(x)).ToArray();
                var checklog = await this._checklogService.QueryDeviceCheckLogWithinRange(device_uids, start.Value, end.Value, 1000);

                List<CheckLogEntity> FindCheckLog(string device_uid, DateTime event_start, DateTime event_end) =>
                checklog
                .Where(x => x.DeviceUID == device_uid)
                .Where(x => x.CreateTime >= event_start && x.CreateTime < event_end).ToList();

                var data = res.Select(x => new
                {
                    x.UID,
                    x.Summary,
                    x.Content,
                    x.DeviceUID,
                    Start = x.DateStart.ToDateString(),
                    End = x.DateEnd.Value.ToDateString(),
                    CheckLog = FindCheckLog(x.DeviceUID, x.DateStart, x.DateEnd.Value),
                    x.Color
                }).ToList();

                return GetJson(new _()
                {
                    success = true,
                    data = data
                });
            });
        }

        /// <summary>
        /// 保存日历事件
        /// </summary>
        /// <param name="org_uid">组织uid</param>
        /// <param name="data">日历时间的json字符串</param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> Save(string data)
        {
            return await RunActionAsync(async () =>
            {
                var model = this.JsonToEntity_<CalendarEventEntity>(data);

                if (model.ByDay != null && model.ByDay.Value > 0)
                {
                    var rrule = new RecurrencePattern(FrequencyType.Daily, model.ByDay.Value);
                    var serializer = new RecurrencePatternSerializer();
                    var s = serializer.SerializeToString(rrule);
                    model.RRule = s;
                }
                model.HasRule = ValidateHelper.IsPlumpString(model.RRule).ToBoolInt();

                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.ManagerRole);

                model.OrgUID = org_uid;
                model.UserUID = loginuser?.UserID;


                if (ValidateHelper.IsPlumpString(model.UID))
                {
                    var res = await this._calService.UpdateEvent(model);
                    if (res.error)
                    {
                        return GetJsonRes(res.msg);
                    }
                }
                else
                {
                    var res = await this._calService.AddEvent(model);
                    if (res.error)
                    {
                        return GetJsonRes(res.msg);
                    }
                }


                return GetJsonRes(string.Empty);
            });
        }

        /// <summary>
        /// 删除事件
        /// </summary>
        /// <param name="org_uid"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> Delete(string uid)
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();

                var res = await this._calService.DeleteEvent(org_uid, uid);
                var loginuser = await this.ValidMember(org_uid, this.ManagerRole);

                if (!res)
                {
                    return GetJsonRes("删除失败");
                }

                return GetJsonRes(string.Empty);
            });
        }

    }
}