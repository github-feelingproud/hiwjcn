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
using Lib.mvc.auth;
using Lib.helper;
using Hiwjcn.Framework;
using Hiwjcn.Service.Epc;

namespace Hiwjcn.Web.Areas.Epc.Controllers
{
    /// <summary>
    /// 页面/guide book
    /// </summary>
    public class PageController : EpcBaseController
    {
        private readonly IPageService _pageService;

        public PageController(IPageService _pageService)
        {
            this._pageService = _pageService;
        }

        /// <summary>
        /// 翟瑞
        /// 显示详情
        /// </summary>
        /// <param name="org_uid"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> QueryPageByUID(string uid)
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.X.context.GetAuthUserAsync();

                var page = await this._pageService.GetPageByUID(uid);
                if (page == null)
                {
                    return GetJsonRes("页面不存在");
                }
                if (page.OrgUID != org_uid)
                {
                    return GetJsonRes("access deny");
                }

                return GetJson(new _()
                {
                    success = page != null,
                    data = page
                });
            });
        }

        /// <summary>
        /// 页面列表
        /// </summary>
        /// <param name="org_uid"></param>
        /// <param name="q"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> Query(string q, int? page)
        {
            return await RunActionAsync(async () =>
            {
                page = CheckPage(page);
                var pagesize = 10;
                var org_uid = this.GetSelectedOrgUID();

                var data = await this._pageService.QueryPager(org_uid, q, page.Value, pagesize);

                return GetJson(new _()
                {
                    success = true,
                    data = data
                });
            });
        }

        /// <summary>
        /// 保存或者编辑
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> Save(string data)
        {
            return await RunActionAsync(async () =>
            {
                var page = data?.JsonToEntity<PageEntity>();
                if (page == null)
                {
                    return GetJsonRes("参数错误");
                }
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.X.context.GetAuthUserAsync();

                page.OrgUID = org_uid;
                page.UserUID = loginuser?.UserID;

                if (ValidateHelper.IsPlumpString(page.UID))
                {
                    var res = await this._pageService.UpdatePage(page);
                    if (res.error)
                    {
                        return GetJsonRes(res.msg);
                    }
                }
                else
                {
                    var res = await this._pageService.AddPage(page);
                    if (res.error)
                    {
                        return GetJsonRes(res.msg);
                    }
                }
                return GetJsonRes(string.Empty);
            });
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> Delete(string uid)
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var res = await this._pageService.DeletePage(uid);
                if (res.error)
                {
                    return GetJsonRes(res.msg);
                }

                return GetJsonRes(string.Empty);
            });
        }

    }
}