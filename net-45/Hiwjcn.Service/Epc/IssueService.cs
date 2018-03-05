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
        Task<_<IssueEntity>> AddIssue(IssueEntity model);

        Task<_<string>> OpenOrClose(string issue_uid, string user_uid, bool close);

        Task<PagerData<IssueEntity>> QueryIssue(string org_uid,
            DateTime? start = null, DateTime? end = null,
            string user_uid = null, string assigned_user_uid = null, bool? open = true,
            string q = null, int page = 1, int pagesize = 10);

        Task<List<IssueOperationLogEntity>> QueryIssueOperationLog(string org_uid, string issue_uid, int count);

        Task<List<IssueEntity>> TopOpenIssue(string org_uid, int count);

        Task<_<IssueOperationLogEntity>> AddComment(IssueOperationLogEntity model);
    }

    public class IssueService : ServiceBase<IssueEntity>, IIssueService
    {
        private readonly IEpcRepository<IssueEntity> _issueRepo;
        private readonly IEpcRepository<IssueOperationLogEntity> _issueOperaRepo;
        private readonly IMSRepository<UserEntity> _userRepo;

        public IssueService(
            IEpcRepository<IssueEntity> _issueRepo,
            IEpcRepository<IssueOperationLogEntity> _issueOperaRepo,
            IMSRepository<UserEntity> _userRepo)
        {
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

            return await this._issueOperaRepo.AddEntity_(model, "op");
        }

        public virtual async Task<_<IssueEntity>> AddIssue(IssueEntity model) =>
            await this._issueRepo.AddEntity_(model, "is");


        public virtual async Task<_<string>> OpenOrClose(string issue_uid, string user_uid, bool close)
        {
            return await this._issueRepo.PrepareSessionAsync(async db =>
            {
                var query = db.Set<IssueEntity>().AsQueryable();
                var res = new _<string>();
                var now = DateTime.Now;

                var issue = await this._issueRepo.GetFirstAsync(x => x.UID == issue_uid);
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

        public virtual async Task<PagerData<IssueEntity>> QueryIssue(string org_uid,
            DateTime? start = null, DateTime? end = null,
            string user_uid = null, string assigned_user_uid = null, bool? open = true,
            string q = null, int page = 1, int pagesize = 10)
        {
            return await this._issueRepo.PrepareSessionAsync(async db =>
            {
                var now = DateTime.Now;
                var query = db.Set<IssueEntity>().AsNoTrackingQueryable();
                query = query.Where(x => x.OrgUID == org_uid);
                query = query.Where(x => x.Start == null || x.Start < now);

                query = query.FilterCreateDateRange(start, end);
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
                if (ValidateHelper.IsPlumpString(user_uid))
                {
                    query = query.Where(x => x.UserUID == user_uid);
                }
                if (ValidateHelper.IsPlumpString(assigned_user_uid))
                {
                    query = query.Where(x => x.AssignedUserUID == assigned_user_uid);
                }
                if (ValidateHelper.IsPlumpString(q))
                {
                    query = query.Where(x => x.Title.Contains(q));
                }

                var data = await query.ToPagedListAsync(page, pagesize, x => x.CreateTime);

                if (ValidateHelper.IsPlumpList(data.DataList))
                {
                    var uids = data.DataList.Select(x => x.DeviceUID).ToList();
                    var devices = await db.Set<DeviceEntity>().AsNoTrackingQueryable().Where(x => uids.Contains(x.UID)).ToListAsync();

                    var user_uids = data.DataList.SelectMany(x => new string[] { x.UserUID, x.AssignedUserUID })
                    .NotEmptyAndDistinct(x => x).ToList();
                    var users = await this._userRepo.GetListAsync(x => user_uids.Contains(x.UID));

                    foreach (var m in data.DataList)
                    {
                        m.DeviceModel = devices.FirstOrDefault(x => x.UID == m.DeviceUID);
                        m.UserModel = users.FirstOrDefault(x => x.UID == m.UserUID);
                        m.AssignedUserModel = users.FirstOrDefault(x => x.UID == m.AssignedUserUID);
                    }
                }

                return data;
            });
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

        public virtual async Task<List<IssueEntity>> TopOpenIssue(string org_uid, int count) =>
            await this._issueRepo.QueryListAsync(
                where: x => x.OrgUID == org_uid,
                orderby: x => x.IID, Desc: true,
                count: count);
    }
}
