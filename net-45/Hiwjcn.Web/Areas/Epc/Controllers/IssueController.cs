using EPC.Core.Entity;
using Hiwjcn.Core;
using Hiwjcn.Framework;
using Hiwjcn.Service.Epc;
using Lib.extension;
using Lib.helper;
using Lib.mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Hiwjcn.Web.Areas.Epc.Controllers
{
    /// <summary>
    /// issue，存在问题
    /// </summary>
    public class IssueController : EpcBaseController
    {
        private readonly IIssueService _issueService;

        public IssueController(
            IIssueService _issueService)
        {
            this._issueService = _issueService;
        }

        [HttpPost, EpcAuth]
        public async Task<ActionResult> QueryIssue(string uid)
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid);

                var data = await this._issueService.QueryIssueByUID(org_uid, uid);

                return GetJson(new _()
                {
                    success = true,
                    data = data
                });
            });
        }

        [HttpPost, EpcAuth]
        public async Task<ActionResult> QueryList(int? max_id,
            string device_uid, string just_open, DateTime? start, DateTime? end)
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.AnyRole);

                var open = default(bool?);
                if ((just_open ?? "false").ToBool())
                {
                    open = true;
                }

                var data = await this._issueService.QueryIssueList(org_uid, max_id, this.PageSize,
                    device_uid: device_uid, start: start, end: end, open: open);

                data = await this._issueService._LoadPagerExtraData(data);

                return GetJson(new _()
                {
                    success = true,
                    data = data
                });
            });
        }

        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> QueryOperationLog(string issue_uid)
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.AnyRole);

                var data = await this._issueService.QueryIssueOperationLog(org_uid, issue_uid, 100);
                return GetJson(new _()
                {
                    success = true,
                    data = data.OrderBy(x => x.IID).ToList()
                });
            });
        }

        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> TopOpenIssue()
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.AnyRole);

                var data = await this._issueService.QueryIssueList(
                    org_uid: org_uid, max_id: null, pagesize: this.PageSize);

                return GetJson(new _()
                {
                    success = true,
                    data = data
                });
            });
        }

        /// <summary>
        /// 我完成的issue
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> MyClosedIssue(int? max_id)
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.AnyRole);

                var data = await this._issueService.MyIssue_(org_uid,
                    user_uid: null,
                    assigned_user_uid: loginuser.UserID,
                    open: false,
                    max_id: max_id,
                    pagesize: this.PageSize);

                data = await _issueService._LoadPagerExtraData(data);

                return GetJson(new _()
                {
                    success = true,
                    data = data
                });
            });
        }

        /// <summary>
        /// 我的待处理
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpPost, EpcAuth]
        public async Task<ActionResult> MyOpenedIssue(int? max_id)
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.AnyRole);

                var data = await this._issueService.MyIssue_(org_uid,
                    user_uid: null,
                    assigned_user_uid: loginuser.UserID,
                    open: true,
                    max_id: max_id,
                    pagesize: this.PageSize);

                data = await _issueService._LoadPagerExtraData(data);

                return GetJson(new _()
                {
                    success = true,
                    data = data
                });
            });
        }

        [HttpPost, EpcAuth]
        public async Task<ActionResult> IssueSendByMe(int? max_id)
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.AnyRole);

                var data = await this._issueService.MyIssue_(org_uid,
                    user_uid: loginuser.UserID,
                    assigned_user_uid: null,
                    open: null,
                    max_id: max_id,
                    pagesize: this.PageSize);
                
                data = await _issueService._LoadPagerExtraData(data);

                return GetJson(new _()
                {
                    success = true,
                    data = data
                });
            });
        }

        /// <summary>
        /// 打开或者关闭
        /// 翟瑞
        /// </summary>
        /// <param name="issue_uid"></param>
        /// <param name="open"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> CloseOrOpen(string issue_uid, string open)
        {
            return await RunActionAsync(async () =>
            {
                if (!ValidateHelper.IsAllPlumpString(issue_uid, open)) { throw new NoParamException(); }

                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.MemberRole);

                var data = await this._issueService.OpenOrClose(issue_uid, loginuser.UserID, !open.ToBool());

                if (data.error)
                {
                    return GetJsonRes(data.msg);
                }

                return GetJsonRes(string.Empty);
            });
        }

        /// <summary>
        /// 翟瑞
        /// 添加修改问题
        /// </summary>
        /// <param name="org_uid"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> Save(string data)
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.MemberRole);

                var model = data?.JsonToEntity<IssueEntity>(throwIfException: false) ?? throw new NoParamException();

                model.OrgUID = org_uid;
                model.UserUID = loginuser.UserID;

                if (ValidateHelper.IsPlumpString(model.UID))
                {
                    throw new NotImplementedException();
                }
                else
                {
                    var res = await this._issueService.AddIssue(model);
                    if (res.error)
                    {
                        return GetJsonRes(res.msg);
                    }
                }

                return GetJsonRes(string.Empty);
            });
        }

        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> Comment(string data)
        {
            return await RunActionAsync(async () =>
            {
                var model = data?.JsonToEntity<IssueOperationLogEntity>(throwIfException: false) ?? throw new NoParamException();

                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.MemberRole);

                model.UserUID = loginuser.UserID;
                model.OrgUID = org_uid;

                var res = await this._issueService.AddComment(model);
                if (res.error)
                {
                    return GetJsonRes(res.msg);
                }

                return GetJsonRes(string.Empty);
            });
        }

    }
}