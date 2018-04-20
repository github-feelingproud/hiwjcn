using EPC.Core;
using EPC.Core.Entity;
using EPC.Core.Model;
using Hiwjcn.Core.Data;
using Hiwjcn.Core.Domain.User;
using Lib.data.ef;
using Lib.helper;
using Lib.infrastructure;
using Lib.infrastructure.extension;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Hiwjcn.Service.Epc
{
    public interface IDashboardService : IServiceBase<CheckLogEntity>
    {
        Task<int> IssueCount(string org_uid, DateTime start, DateTime end);

        Task<int> DeviceCount(string org_uid);

        Task<int> CheckLogCount(string org_uid, DateTime start, DateTime end);

        Task<List<IssueGroupBy>> IssueCountGroupByStatus(string org_uid, DateTime start, DateTime end);

        Task<List<IssueGroupBy>> IssueCountGroupByDay(string org_uid,
            DateTime start, DateTime end, string device_uid = null);

        Task<List<IssueGroupBy>> IssueCountGroupByDevice(string org_uid,
            DateTime start, DateTime end, int count = 10);

        Task<List<IssueGroupBy>> IssueResolveTimeTakeGroupByDay(string org_uid,
            DateTime start, DateTime end, string device_uid = null, string user_uid = null);

        Task<List<CheckLogGroupBy>> DeviceLastCheckTime(string org_uid, string[] device_uid, DateTime recent);

        Task<List<CheckLogGroupBy>> CheckLogGroupByUser(string org_uid, DateTime start, DateTime end, int top_count);
    }

    public class DashboardService : ServiceBase<CheckLogEntity>, IDashboardService
    {
        private readonly IEpcRepository<CheckLogEntity> _logRepo;
        private readonly IEpcRepository<CheckLogItemEntity> _logItemRepo;
        private readonly IMSRepository<UserEntity> _userRepo;

        public DashboardService(
            IEpcRepository<CheckLogEntity> _logRepo,
            IEpcRepository<CheckLogItemEntity> _logItemRepo,
            IMSRepository<UserEntity> _userRepo)
        {
            this._logRepo = _logRepo;
            this._logItemRepo = _logItemRepo;
            this._userRepo = _userRepo;
        }

        public async Task<List<IssueGroupBy>> IssueCountGroupByStatus(string org_uid, DateTime start, DateTime end)
        {
            return await this._logRepo.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;
                var query = db.Set<IssueEntity>().AsNoTrackingQueryable();
                query = query.Where(x => x.OrgUID == org_uid);
                query = query.FilterCreateDateRange(start, end, true);
                query = query.Where(x => x.Start == null || x.Start < now);

                var data = await query
                .GroupBy(x => x.IsClosed)
                .Select(x => new
                {
                    IsClosed = x.Key,
                    Count = x.Count()
                }).ToListAsync();

                return data.Select(x => new IssueGroupBy()
                {
                    IsClosed = x.IsClosed,
                    Count = x.Count
                }).ToList();
            });
        }

        public async Task<List<IssueGroupBy>> IssueCountGroupByDay(string org_uid,
            DateTime start, DateTime end, string device_uid = null)
        {
            return await this._logRepo.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;
                var query = db.Set<IssueEntity>().AsNoTrackingQueryable();
                query = query.Where(x => x.OrgUID == org_uid);
                query = query.FilterCreateDateRange(start, end, true);
                query = query.Where(x => x.Start == null || x.Start < now);

                if (ValidateHelper.IsPlumpString(device_uid))
                {
                    query = query.Where(x => x.DeviceUID == device_uid);
                }

                var data = await query
                .GroupBy(x => new { x.TimeYear, x.TimeMonth, x.TimeDay, x.IsClosed })
                .Select(x => new
                {
                    x.Key.TimeYear,
                    x.Key.TimeMonth,
                    x.Key.TimeDay,
                    x.Key.IsClosed,
                    Count = x.Count()
                }).ToListAsync();

                return data.Select(x => new IssueGroupBy()
                {
                    Year = x.TimeYear,
                    Month = x.TimeMonth,
                    Day = x.TimeDay,
                    IsClosed = x.IsClosed,
                    Count = x.Count
                }).ToList();
            });
        }

        public async Task<List<IssueGroupBy>> IssueCountGroupByDevice(string org_uid,
            DateTime start, DateTime end, int count = 10)
        {
            return await this._logRepo.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;
                var query = db.Set<IssueEntity>().AsNoTrackingQueryable();
                query = query.Where(x => x.OrgUID == org_uid);
                query = query.FilterCreateDateRange(start, end, true);
                query = query.Where(x => x.DeviceUID != null && x.DeviceUID != string.Empty);
                query = query.Where(x => x.Start == null || x.Start < now);

                var data = await query
                .GroupBy(x => x.DeviceUID)
                .Select(x => new
                {
                    DeviceUID = x.Key,
                    Count = x.Count()
                }).OrderByDescending(x => x.Count).Take(count).ToListAsync();

                var list = data.Select(x => new IssueGroupBy()
                {
                    DeviceUID = x.DeviceUID,
                    Count = x.Count
                }).ToList();

                if (ValidateHelper.IsPlumpList(list))
                {
                    var uids = list.Select(x => x.DeviceUID).ToList();
                    var devices = await db.Set<DeviceEntity>().AsNoTrackingQueryable().Where(x => uids.Contains(x.UID)).ToListAsync();
                    foreach (var m in list)
                    {
                        m.DeviceModel = devices.Where(x => x.UID == m.DeviceUID).FirstOrDefault();
                    }
                }

                return list;
            });
        }

        public async Task<List<IssueGroupBy>> IssueResolveTimeTakeGroupByDay(string org_uid,
            DateTime start, DateTime end, string device_uid = null, string user_uid = null)
        {
            return await this._logRepo.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;
                var query = db.Set<IssueEntity>().AsNoTrackingQueryable();
                query = query.Where(x => x.OrgUID == org_uid);
                query = query.FilterCreateDateRange(start, end, true);
                query = query.Where(x => x.IsClosed > 0);
                query = query.Where(x => x.Start == null || x.Start < now);
                if (ValidateHelper.IsPlumpString(device_uid))
                {
                    query = query.Where(x => x.DeviceUID == device_uid);
                }
                if (ValidateHelper.IsPlumpString(user_uid))
                {
                    query = query.Where(x => x.UserUID == user_uid);
                }

                var data = await query.GroupBy(x => new
                {
                    x.TimeYear,
                    x.TimeMonth,
                    x.TimeDay
                }).Select(x => new
                {
                    x.Key.TimeYear,
                    x.Key.TimeMonth,
                    x.Key.TimeDay,
                    AvgSecondsToTakeToClose = x.Average(m => m.SecondsToTakeToClose)
                }).ToListAsync();

                return data.Select(x => new IssueGroupBy()
                {
                    Year = x.TimeYear,
                    Month = x.TimeMonth,
                    Day = x.TimeDay,
                    AvgSecondsToTakeToClose = (int)x.AvgSecondsToTakeToClose
                }).ToList();
            });
        }

        public async Task<List<CheckLogGroupBy>> DeviceLastCheckTime(string org_uid, string[] device_uid, DateTime recent)
        {
            return await this._logRepo.PrepareSessionAsync(async db =>
            {
                var query = db.Set<CheckLogEntity>().AsNoTrackingQueryable();
                query = query.Where(x => x.OrgUID == org_uid && x.CreateTime > recent && device_uid.Contains(x.DeviceUID));

                var data = await query
                .GroupBy(x => x.DeviceUID)
                .Select(x => new
                {
                    DeviceUID = x.Key,
                    LastCheckTime = x.Max(m => m.CreateTime)
                }).ToListAsync();

                return data.Select(x => new CheckLogGroupBy()
                {
                    DeviceUID = x.DeviceUID,
                    LastCheckTime = x.LastCheckTime
                }).ToList();
            });
        }

        public async Task<int> IssueCount(string org_uid, DateTime start, DateTime end)
        {
            return await this._logRepo.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;
                var query = db.Set<IssueEntity>().AsNoTrackingQueryable();

                query = query.Where(x => x.OrgUID == org_uid);
                query = query.Where(x => x.CreateTime >= start && x.CreateTime < end);
                query = query.Where(x => x.Start == null || x.Start < now);

                return await query.CountAsync();
            });
        }

        public async Task<int> DeviceCount(string org_uid)
        {
            return await this._logRepo.PrepareSessionAsync(async db =>
            {
                var query = db.Set<DeviceEntity>().AsNoTrackingQueryable();

                query = query.Where(x => x.OrgUID == org_uid);

                return await query.CountAsync();
            });
        }

        public async Task<int> CheckLogCount(string org_uid, DateTime start, DateTime end)
        {
            return await this._logRepo.PrepareSessionAsync(async db =>
            {
                var query = db.Set<CheckLogEntity>().AsNoTrackingQueryable();

                query = query.Where(x => x.OrgUID == org_uid);
                query = query.Where(x => x.CreateTime >= start && x.CreateTime < end);

                return await query.CountAsync();
            });
        }

        public async Task<List<CheckLogGroupBy>> CheckLogGroupByUser(string org_uid, DateTime start, DateTime end, int top_count)
        {
            return await this._logRepo.PrepareSessionAsync(async db =>
            {
                var query = db.Set<CheckLogEntity>().AsNoTrackingQueryable();

                query = query.Where(x => x.OrgUID == org_uid);
                query = query.Where(x => x.CreateTime >= start && x.CreateTime < end);

                var data = await query.GroupBy(x => x.UserUID)
                .Select(x => new { x.Key, Count = x.Count() })
                .OrderByDescending(x => x.Count)
                .Take(top_count).ToListAsync();

                var list = data.Select(x => new CheckLogGroupBy()
                {
                    UserUID = x.Key,
                    Count = x.Count
                }).ToList();

                if (ValidateHelper.IsPlumpList(list))
                {
                    var userids = list.Select(x => x.UserUID).ToArray();
                    var userlist = await this._userRepo.GetListAsync(x => userids.Contains(x.UID));
                    foreach (var m in list)
                    {
                        m.UserModel = userlist.FirstOrDefault(x => x.UID == m.UserUID);
                    }
                }

                return list;
            });
        }
    }

}
