using EPC.Core;
using EPC.Core.Entity;
using Hiwjcn.Core.Data;
using Hiwjcn.Core.Domain.User;
using Lib.data.ef;
using Lib.extension;
using Lib.helper;
using Lib.infrastructure;
using Lib.infrastructure.extension;
using Lib.mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Hiwjcn.Service.Epc
{
    public interface IIssueService : IServiceBase<IssueEntity>
    {
        Task<IssueEntity> QueryIssueByUID(string org_uid, string uid);

        Task<_<IssueEntity>> AddIssue(IssueEntity model);

        Task<_<string>> OpenOrClose(string issue_uid, string user_uid, bool close);

        Task<List<IssueEntity>> QueryIssueList(string org_uid, int? max_id, int pagesize,
            string device_uid = null, DateTime? start = null, DateTime? end = null, bool? open = null);

        [Obsolete("不使用count分页")]
        Task<PagerData<IssueEntity>> MyIssue(
            string org_uid, string user_uid, string assigned_user_uid, bool? open,
            int page, int pagesize);

        Task<List<IssueEntity>> MyIssue_(
            string org_uid, string user_uid, string assigned_user_uid, bool? open,
            int? max_id, int pagesize);

        Task<List<IssueOperationLogEntity>> QueryIssueOperationLog(string org_uid, string issue_uid, int count);

        Task<_<IssueOperationLogEntity>> AddComment(IssueOperationLogEntity model);

        Task<List<IssueEntity>> _LoadPagerExtraData(List<IssueEntity> data);
    }

    public class IssueService : ServiceBase<IssueEntity>, IIssueService
    {
        private readonly IEpcRepository<DeviceEntity> _deviceRepo;
        private readonly IEpcRepository<IssueEntity> _issueRepo;
        private readonly IEpcRepository<IssueOperationLogEntity> _issueOperaRepo;
        private readonly IMSRepository<UserEntity> _userRepo;

        public IssueService(
            IEpcRepository<DeviceEntity> _deviceRepo,
            IEpcRepository<IssueEntity> _issueRepo,
            IEpcRepository<IssueOperationLogEntity> _issueOperaRepo,
            IMSRepository<UserEntity> _userRepo)
        {
            this._deviceRepo = _deviceRepo;
            this._issueRepo = _issueRepo;
            this._issueOperaRepo = _issueOperaRepo;
            this._userRepo = _userRepo;
        }

        public async Task<_<IssueOperationLogEntity>> AddComment(IssueOperationLogEntity model)
        {
            var issue = await this._issueRepo.GetFirstAsync(x => x.UID == model.IssueUID);
            issue = issue ?? throw new ArgumentNullException("issue is null");
            if (issue.OrgUID != model.OrgUID) { throw new Exception("org is wrong"); }

            var res = new _<IssueOperationLogEntity>();
            if (issue.IsClosed > 0)
            {
                res.SetErrorMsg("issue已经被关闭，无法评论");
                return res;
            }

            res = await this._issueOperaRepo.AddEntity_(model, "op");

            {
                //更新评论数
                var status = new string[] { "open", "close" };
                issue.CommentCount = await this._issueOperaRepo.GetCountAsync(x =>
                x.OrgUID == issue.OrgUID && x.IssueUID == issue.UID && !status.Contains(x.Operation));
                await this._issueRepo.UpdateAsync(issue);
            }

            return res;
        }

        public virtual async Task<_<IssueEntity>> AddIssue(IssueEntity model) =>
            await this._issueRepo.AddEntity_(model, "is");

        public async Task<List<IssueEntity>> MyIssue_(
            string org_uid, string user_uid, string assigned_user_uid, bool? open,
            int? max_id, int pagesize)
        {
            return await this._issueRepo.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;
                var query = db.Set<IssueEntity>().AsNoTrackingQueryable();
                query = query.Where(x => x.OrgUID == org_uid);
                //对于客户端，所有issue都显示，即便没有超过5分钟
                //query = query.Where(x => x.Start == null || x.Start < now);

                if (max_id != null && max_id.Value >= 0)
                {
                    query = query.Where(x => x.IID < max_id);
                }

                if (ValidateHelper.IsPlumpString(user_uid))
                {
                    query = query.Where(x => x.UserUID == user_uid);
                }
                if (ValidateHelper.IsPlumpString(assigned_user_uid))
                {
                    query = query.Where(x => x.AssignedUserUID == assigned_user_uid);
                }
                if (open != null)
                {
                    if (open.Value)
                    {
                        query = query.Where(x => x.IsClosed <= 0);
                    }
                    else
                    {
                        query = query.Where(x => x.IsClosed > 0);
                    }
                }

                var data = await query.OrderByDescending(x => x.IID).Take(pagesize).ToListAsync();

                return data;
            });
        }

        public async Task<PagerData<IssueEntity>> MyIssue(string org_uid,
            string user_uid, string assigned_user_uid, bool? open,
            int page, int pagesize)
        {
            return await this._issueRepo.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;
                var query = db.Set<IssueEntity>().AsNoTrackingQueryable();
                query = query.Where(x => x.OrgUID == org_uid);
                //对于客户端，所有issue都显示，即便没有超过5分钟
                //query = query.Where(x => x.Start == null || x.Start < now);

                if (ValidateHelper.IsPlumpString(user_uid))
                {
                    query = query.Where(x => x.UserUID == user_uid);
                }
                if (ValidateHelper.IsPlumpString(assigned_user_uid))
                {
                    query = query.Where(x => x.AssignedUserUID == assigned_user_uid);
                }
                if (open != null)
                {
                    if (open.Value)
                    {
                        query = query.Where(x => x.IsClosed <= 0);
                    }
                    else
                    {
                        query = query.Where(x => x.IsClosed > 0);
                    }
                }

                var data = new PagerData<IssueEntity>();
                data.Page = page;
                data.PageSize = pagesize;
                data.ItemCount = await query.CountAsync();
                data.DataList = await query.OrderBy(x => x.IsClosed).OrderByDescending(x => x.CreateTime).ToListAsync();

                return data;
            });
        }

        public virtual async Task<_<string>> OpenOrClose(string issue_uid, string user_uid, bool close)
        {
            return await this._issueRepo.PrepareSessionAsync(async db =>
            {
                var query = db.Set<IssueEntity>().AsQueryable();
                var res = new _<string>();
                var now = DateTime.Now;

                var issue = await query.FirstOrDefaultAsync(x => x.UID == issue_uid);
                Com.AssertNotNull(issue, "issue is null");

                if (issue.AssignedUserUID != user_uid && issue.UserUID != user_uid)
                {
                    res.SetErrorMsg("无权操作");
                    return res;
                }
                var status = close.ToBoolInt();
                if (issue.IsClosed == status)
                {
                    res.SetErrorMsg("状态无需改变");
                    return res;
                }
                issue.IsClosed = status;
                if (close)
                {
                    var seconds = (int)(now - (issue.Start ?? issue.CreateTime)).TotalSeconds;
                    if (seconds >= 0)
                    {
                        //处理问题花了多长时间
                        issue.SecondsToTakeToClose = seconds;
                    }
                }
                else
                {
                    issue.SecondsToTakeToClose = 0;
                }

                var log = new IssueOperationLogEntity()
                {
                    UserUID = user_uid,
                    OrgUID = issue.OrgUID,
                    IssueUID = issue.UID,
                    IsClosed = issue.IsClosed,
                    Operation = close ? "close" : "open",
                    Content = string.Empty
                }.InitSelf("op");

                db.Set<IssueOperationLogEntity>().Add(log);

                await db.SaveChangesAsync();

                res.SetSuccessData(string.Empty);
                return res;
            });
        }

        public virtual async Task<List<IssueEntity>> QueryIssueList(string org_uid, int? max_id, int pagesize,
            string device_uid = null, DateTime? start = null, DateTime? end = null, bool? open = null)
        {
            return await this._issueRepo.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;
                var query = db.Set<IssueEntity>().AsNoTrackingQueryable();
                query = query.Where(x => x.OrgUID == org_uid);
                query = query.Where(x => x.Start == null || x.Start < now);
                query = query.FilterCreateDateRange(start, end);

                if (max_id != null && max_id.Value >= 0)
                {
                    query = query.Where(x => x.IID < max_id);
                }

                if (open != null)
                {
                    if (open.Value)
                    {
                        query = query.Where(x => x.IsClosed <= 0);
                    }
                    else
                    {
                        query = query.Where(x => x.IsClosed > 0);
                    }
                }
                if (ValidateHelper.IsPlumpString(device_uid))
                {
                    query = query.Where(x => x.DeviceUID == device_uid);
                }

                var data = await query.OrderByDescending(x => x.IID).Take(pagesize).ToListAsync();

                return data;
            });
        }

        public async Task<List<IssueEntity>> _LoadPagerExtraData(List<IssueEntity> data)
        {
            if (ValidateHelper.IsPlumpList(data))
            {
                var device_uids = data.Select(x => x.DeviceUID).ToList();
                var devices = await this._deviceRepo.GetListAsync(x => device_uids.Contains(x.UID));

                var user_uids = data.SelectMany(x => new string[] { x.UserUID, x.AssignedUserUID }).NotEmptyAndDistinct(x => x).ToList();
                var users = await this._userRepo.GetListAsync(x => user_uids.Contains(x.UID));

                foreach (var m in data)
                {
                    m.DeviceModel = devices.FirstOrDefault(x => x.UID == m.DeviceUID);
                    m.UserModel = users.FirstOrDefault(x => x.UID == m.UserUID);
                    m.AssignedUserModel = users.FirstOrDefault(x => x.UID == m.AssignedUserUID);
                }
            }
            return data;
        }

        public virtual async Task<List<IssueOperationLogEntity>> QueryIssueOperationLog(
            string org_uid, string issue_uid, int count)
        {
            var data = await this._issueOperaRepo.QueryListAsync(
                where: x => x.OrgUID == org_uid && x.IssueUID == issue_uid,
                orderby: x => x.IID, Desc: true,
                count: count);

            if (ValidateHelper.IsPlumpList(data))
            {
                var user_uids = data.Select(x => x.UserUID).Distinct().ToList();
                var users = await this._userRepo.GetListAsync(x => user_uids.Contains(x.UID));
                foreach (var m in data)
                {
                    m.UserModel = users.FirstOrDefault(x => x.UID == m.UserUID);
                }
            }

            return data;
        }

        public async Task<IssueEntity> QueryIssueByUID(string org_uid, string uid) =>
            await this._issueRepo.GetFirstAsync(x => x.OrgUID == org_uid && x.UID == uid);
    }
}
