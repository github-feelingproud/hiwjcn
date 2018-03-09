using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lib.mvc;
using System.Threading.Tasks;
using Hiwjcn.Service;
using Lib.infrastructure.helper;
using Lib.infrastructure.model;
using Lib.extension;
using EPC.Core.Entity;
using Lib.helper;
using Hiwjcn.Framework;
using Hiwjcn.Service.Epc;
using Hiwjcn.Core;

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

        /// <summary>
        /// 显示列表
        /// </summary>
        /// <param name="org_uid"></param>
        /// <param name="q"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> Query(string q, string just_open, DateTime? start, DateTime? end, int? page)
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

                var data = await this._issueService.QueryIssue(org_uid,
                    start: start, end: end,
                    open: open, q: q,
                    page: page.Value, pagesize: this.PageSize);

                data.DataList = await this._issueService._LoadPagerExtraData(data.DataList);

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

                var data = await this._issueService.TopOpenIssue(org_uid, count: 10);

                return GetJson(new _()
                {
                    success = true,
                    data = data
                });
            });
        }

        /// <summary>
        /// 翟瑞
        /// 我的问题
        /// </summary>
        /// <param name="org_uid"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> MyIssue(string byme, int? page)
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.AnyRole);

                page = this.CheckPage(page);

                var user_uid = string.Empty;
                var assigned_user_uid = string.Empty;

                if ((byme ?? "false").ToBool())
                {
                    //我开启的
                    user_uid = loginuser.UserID;
                }
                else
                {
                    //指派给我的
                    assigned_user_uid = loginuser.UserID;
                }

                var data = await this._issueService.MyIssue(org_uid,
                    user_uid: user_uid, assigned_user_uid: assigned_user_uid,
                    page: page.Value, pagesize: this.PageSize);

                data.DataList = await _issueService._LoadPagerExtraData(data.DataList);

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
                if (!ValidateHelper.IsAllPlumpString(issue_uid, open)) { return GetJsonRes("参数错误"); }

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