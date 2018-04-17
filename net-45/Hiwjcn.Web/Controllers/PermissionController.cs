using Hiwjcn.Core;
using Hiwjcn.Core.Domain.User;
using Hiwjcn.Framework;
using Hiwjcn.Service.MemberShip;
using Lib.extension;
using Lib.helper;
using Lib.infrastructure.extension;
using Lib.infrastructure.helper;
using Lib.infrastructure.model;
using Lib.mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Hiwjcn.Web.Controllers
{
    public class PermissionController : EpcBaseController
    {
        private readonly IPermissionService _perService;

        public PermissionController(
            IPermissionService _perService)
        {
            this._perService = _perService;
        }

        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> Query()
        {
            return await RunActionAsync(async () =>
            {
                var list = await this._perService.QueryPermissionList();
                var iviewdata = list.Select(x => (IViewTreeNode)x).ToList();

                foreach (var m in iviewdata)
                {
                    m.expand = false;
                }

                return GetJson(new _()
                {
                    success = true,
                    data = TreeHelper.BuildIViewTreeStructure(iviewdata)
                });
            });
        }

        /// <summary>
        /// 删除权限
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpPost]
        //[EpcAuth(Permission = "manage.per.delete")]
        [EpcAuth]
        public async Task<ActionResult> Delete(string uid)
        {
            return await RunActionAsync(async () =>
            {
                var res = await this._perService.DeletePermissionWhenNoChildren(uid);
                if (res.error)
                {
                    return GetJsonRes(res.msg);
                }

                return GetJsonRes(string.Empty);
            });
        }

        /// <summary>
        /// 添加权限
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        //[EpcAuth(Permission = "manage.per.edit")]
        [EpcAuth]
        public async Task<ActionResult> Save(string data)
        {
            return await RunActionAsync(async () =>
            {
                var model = this.JsonToEntity_<PermissionEntity>(data);

                if (ValidateHelper.IsPlumpString(model.UID))
                {
                    var res = await this._perService.UpdatePermission(model);
                    if (res.error)
                    {
                        return GetJsonRes(res.msg);
                    }
                }
                else
                {
                    model.AsFirstLevelIfParentIsNotValid();
                    var res = await this._perService.AddPermission(model);
                    if (res.error)
                    {
                        return GetJsonRes(res.msg);
                    }
                }

                return GetJsonRes(string.Empty);
            });
        }

    }
}