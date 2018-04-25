using EPC.Core;
using EPC.Core.Entity;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Lib.data.ef;
using Lib.extension;
using Lib.helper;
using Lib.infrastructure;
using Lib.mvc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Hiwjcn.Service.Epc
{
    public interface ICalendarService : IServiceBase<CalendarEventEntity>
    {
        Task<int> GetMaxRRuleCount(string org_uid);

        Task<List<CalendarEventEntity>> GetAllEventRule(string org_uid);

        Task<bool> DeleteEvent(string org_uid, string uid);

        Task<_<string>> AddEvent(CalendarEventEntity model);

        Task<_<string>> UpdateEvent(CalendarEventEntity model);

        Task<List<CalendarEventEntity>> QueryEvents(string org_uid, DateTime start, DateTime end);
    }

    public class CalendarService : ServiceBase<CalendarEventEntity>,
        ICalendarService
    {
        private readonly int MaxRRuleCount = (ConfigurationManager.AppSettings["MaxRRuleCount"] ?? "100").ToInt(100);

        private readonly IEpcRepository<CalendarEventEntity> _calendarRepo;

        public CalendarService(
            IEpcRepository<CalendarEventEntity> _calendarRepo)
        {
            this._calendarRepo = _calendarRepo;
        }

        public virtual async Task<int> GetMaxRRuleCount(string org_uid) => await Task.FromResult(this.MaxRRuleCount);
        
        public virtual async Task<bool> DeleteEvent(string org_uid, string uid)
        {
            if (!ValidateHelper.IsAllPlumpString(org_uid, uid)) { throw new Exception("参数错误"); }
            var deleted = await this._calendarRepo.DeleteWhereAsync(x => x.OrgUID == org_uid && x.UID == uid);
            return deleted > 0;
        }

        public virtual async Task<_<string>> AddEvent(CalendarEventEntity model)
        {
            var data = new _<string>();

            model.Init("evt");

            if (!model.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }

            model.DateStart = model.DateStart.Date;
            model.DateEnd = model.DateEnd?.Date;

            if (await this._calendarRepo.GetCountAsync(x => x.OrgUID == model.OrgUID && x.HasRule > 0) >= await this.GetMaxRRuleCount(model.OrgUID))
            {
                data.SetErrorMsg("规则数量达到上限");
                return data;
            }

            if (await this._calendarRepo.AddAsync(model) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }
            throw new Exception("保存事件失败");
        }

        public virtual async Task<_<string>> UpdateEvent(CalendarEventEntity model)
        {
            var data = new _<string>();

            var e = await this._calendarRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(e, "事件不存在");
            e.Summary = model.Summary;
            e.Content = model.Content;
            e.DeviceUID = model.DeviceUID;
            e.RRule = model.RRule;
            e.HasRule = ValidateHelper.IsPlumpString(model.RRule).ToBoolInt();
            e.DateStart = model.DateStart.Date;
            e.DateEnd = model.DateEnd?.Date;
            e.Update();

            if (!e.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }

            if (await this._calendarRepo.UpdateAsync(e) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("更新失败");
        }

        public virtual async Task<List<CalendarEventEntity>> QueryEvents(string org_uid, DateTime start, DateTime end)
        {
            start = start.Date;
            end = end.Date.AddDays(1);
            if (start >= end) { throw new Exception("开始时间，结束时间参数错误"); }
            if ((end - start).TotalDays > 365) { throw new Exception("日期范围过大"); }

            var errors = new List<string>();

            var list = new List<CalendarEventEntity>();

            //有开始结束的事件
            var static_data = await this._calendarRepo.GetListAsync(x => x.OrgUID == org_uid && x.HasRule <= 0 && x.DateEnd != null && x.DateStart < end && x.DateEnd > start);
            list.AddRange(static_data);

            //有规则的事件
            var all_rule_data = await this._calendarRepo.GetListAsync(x => x.OrgUID == org_uid && x.HasRule > 0 && x.RRule != null);
            foreach (var x in all_rule_data)
            {
                if (!ValidateHelper.IsPlumpString(x.RRule))
                {
                    errors.Add($"数据不包含rrule规则，{x.ToJson()}");
                    continue;
                }

                try
                {
                    var calendar_event = new CalendarEvent()
                    {
                        Uid = x.UID,
                        Priority = x.Priority,
                        RecurrenceRules = new List<RecurrencePattern>() { new RecurrencePattern(x.RRule) },
                        Summary = x.Summary,
                        Description = x.Content,
                        Start = new CalDateTime(x.DateStart),
                        Location = x.Location
                    };
                    if (x.DateEnd != null)
                    {
                        calendar_event.End = new CalDateTime(x.DateEnd.Value);
                    }

                    var es = calendar_event.GetOccurrences(new CalDateTime(start), new CalDateTime(end)).ToList();

                    foreach (var e in es)
                    {
                        //复制一份
                        var m = x.ToJson().JsonToEntity<CalendarEventEntity>();
                        m.DateStart = e.Period.StartTime.AsSystemLocal;
                        m.DateEnd = m.DateStart + e.Period.Duration;

                        list.Add(m);
                    }
                }
                catch (Exception e)
                {
                    errors.Add(e.GetInnerExceptionAsJson() + $"计算rrule错误，数据：{x.ToJson()}");
                }
            }

            errors.AddErrorLogIfNotEmpty();

            return list.OrderBy(x => x.DateStart).ToList();
        }

        public async Task<List<CalendarEventEntity>> GetAllEventRule(string org_uid)
        {
            var data = await this._calendarRepo.GetListEnsureMaxCountAsync(x => x.OrgUID == org_uid, 1000, "事件数量达到上限");
            return data.OrderByDescending(x => x.IID).ToList();
        }
    }
}
