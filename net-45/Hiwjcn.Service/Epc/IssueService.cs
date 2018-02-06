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

namespace EPC.Service
{
    public interface IIssueService : IServiceBase<IssueEntity>
    {
        Task<_<IssueEntity>> AddIssue(IssueEntity model);

        Task<_<string>> OpenOrClose(string org_uid, string uid, bool close, string user_uid, string comment);

        Task<PagerData<IssueEntity>> QueryIssue(string org_uid,
            DateTime? start = null, DateTime? end = null,
            string user_uid = null, string assigned_user_uid = null, bool? open = true,
            string q = null, int page = 1, int pagesize = 10);

        Task<List<IssueOperationLogEntity>> QueryIssueOperationLog(string org_uid, string issue_uid, int count);

        Task<List<IssueEntity>> TopOpenIssue(string org_uid, int count);
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

        public virtual async Task<_<IssueEntity>> AddIssue(IssueEntity model) =>
            await this._issueRepo.AddEntity_(model, "is");

        public virtual async Task<_<string>> OpenOrClose(string org_uid, string uid, bool close, string user_uid, string comment)
        {
            var res = new _<string>();

            var model = await this._issueRepo.GetFirstAsync(x => x.UID == uid);
            Com.AssertNotNull(model, "issue不存在");
            if (model.OrgUID != org_uid)
            {
                res.SetErrorMsg("无权操作");
                return res;
            }

            var status = close.ToBoolInt();
            if (model.IsClosed == status)
            {
                res.SetErrorMsg("状态无需改变");
                return res;
            }
            model.IsClosed = status;

            if (close)
            {
                model.SecondsToTakeToClose = (int)(DateTime.Now - model.CreateTime).TotalSeconds;
            }
            else
            {
                model.SecondsToTakeToClose = 0;
            }

            await this._issueRepo.UpdateAsync(model);
            //add log
            var operation_log = new IssueOperationLogEntity()
            {
                OrgUID = model.OrgUID,
                IssueUID = model.UID,
                IsClosed = model.IsClosed,
                UserUID = user_uid,
                Content = comment
            }.InitSelf("iol");
            if (!operation_log.IsValid(out var msg))
            {
                res.SetErrorMsg(msg);
                return res;
            }
            await this._issueOperaRepo.AddAsync(operation_log);

            res.SetSuccessData(string.Empty);
            return res;
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
