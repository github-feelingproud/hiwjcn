using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lib.mvc;
using System.Threading.Tasks;
using EPC.Service;
using Lib.infrastructure.helper;
using Lib.infrastructure.model;
using Lib.extension;
using EPC.Core.Entity;
using Lib.mvc.auth;
using Lib.helper;
using Hiwjcn.Framework;

namespace Hiwjcn.Web.Areas.Epc.Controllers
{
    /// <summary>
    /// 设备
    /// </summary>
    public class DeviceController : EpcBaseController
    {
        private readonly IDeviceService _deviceService;

        public DeviceController(
            IDeviceService _deviceService)
        {
            this._deviceService = _deviceService;
        }

        /// <summary>
        /// 翟瑞
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        [Obsolete, NonAction]
        public async Task<ActionResult> GetDeviceByUID(string uid)
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var data = await this._deviceService.GetDeviceByUID(org_uid, uid);

                return GetJson(new _()
                {
                    success = true,
                    data = data
                });
            });
        }

        /// <summary>
        /// 设备列表
        /// 翟瑞
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
                page = this.CheckPage(page);

                var org_uid = this.GetSelectedOrgUID();
                var pager = await this._deviceService.QueryDevice(org_uid, q, page.Value, this.PageSize);

                return GetJson(new _()
                {
                    success = true,
                    data = pager
                });
            });
        }

        /// <summary>
        /// 保存设备，这个数据结构比较复杂，做之前和我商量
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

                var model = data?.JsonToEntity<DeviceEntity>(throwIfException: false);
                if (model == null)
                {
                    return GetJsonRes("参数错误");
                }
                if (!ValidateHelper.IsPlumpList(model.ParamsList))
                {
                    return GetJsonRes("设备必须配置参数");
                }
                var loginuser = await this.X.context.GetAuthUserAsync();
                model.UserUID = loginuser?.UserID;
                model.OrgUID = org_uid;
                model.ParamsList = ConvertHelper.NotNullList(model.ParamsList);
                if (ValidateHelper.IsPlumpString(model.UID))
                {
                    var res = await this._deviceService.UpdateDevice(model);
                    if (res.error)
                    {
                        return GetJsonRes(res.msg);
                    }
                }
                else
                {
                    var res = await this._deviceService.AddDevice(model);
                    if (res.error)
                    {
                        return GetJsonRes(res.msg);
                    }
                }

                return GetJsonRes(string.Empty);
            });
        }

        /// <summary>
        /// 删除设备
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

                var res = await this._deviceService.DeleteDevice(org_uid, uid);
                if (res.error)
                {
                    return GetJsonRes(res.msg);
                }

                return GetJsonRes(string.Empty);
            });
        }

    }
}