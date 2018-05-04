using EPC.Core.Entity;
using Hiwjcn.Core;
using Hiwjcn.Framework;
using Hiwjcn.Service.Epc;
using Lib.helper;
using Lib.mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

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
        public async Task<ActionResult> GetDeviceByUID(string uid)
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.AnyRole);

                var model = await this._deviceService.GetDeviceByUID(org_uid, uid);

                model = (await this._deviceService._LoadDeviceExtraData(new List<DeviceEntity>() { model })).First();

                return GetJson(new _()
                {
                    success = true,
                    data = model
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
                var loginuser = await this.ValidMember(org_uid, this.AnyRole);

                var data = await this._deviceService.QueryDevice(org_uid, q, page.Value, this.PageSize);

                data.DataList = await this._deviceService._LoadDeviceExtraData(data.DataList);

                return GetJson(new _()
                {
                    success = true,
                    data = data
                });
            });
        }

        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> QueryAll(string q, int? page)
        {
            return await RunActionAsync(async () =>
            {
                page = this.CheckPage(page);

                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.AnyRole);

                var data = await this._deviceService.QueryAll(org_uid);

                data = await this._deviceService._LoadDeviceExtraData(data);

                return GetJson(new _()
                {
                    success = true,
                    data = data
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
                var loginuser = await this.ValidMember(org_uid, this.ManagerRole);
                
                var model = this.JsonToEntity_<DeviceEntity>(data);

                if (!ValidateHelper.IsPlumpList(model.ParamsList))
                {
                    throw new NoParamException();
                }

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
                var loginuser = await this.ValidMember(org_uid, this.ManagerRole);

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