using Hiwjcn.Bll.User;
using Hiwjcn.Core.Domain.User;
using Hiwjcn.Core.Domain;
using Hiwjcn.Framework;
using Lib.extension;
using Lib.mvc.auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Lib.mvc;
using WebCore.MvcLib.Controller;
using Hiwjcn.Bll.Common;
using Lib.infrastructure.model;
using Lib.helper;
using Lib.infrastructure.helper;

namespace Hiwjcn.Web.Controllers
{
    public class MenuController : UserBaseController
    {
        private readonly IMenuService _menuService;

        public MenuController(
            IMenuService _menuService)
        {
            this._menuService = _menuService;
        }

        [HttpPost]
        [EpcAuth(Permission = "manage.menu.query")]
        public async Task<ActionResult> Query(string group)
        {
            return await RunActionAsync(async () =>
            {
                var list = await this._menuService.QueryMenuList(group);
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

        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> ShowMenu(string group)
        {
            return await RunActionAsync(async () =>
            {
                var list = await this._menuService.QueryMenuList(group);
                return GetJson(new _()
                {
                    success = true,
                    data = list
                });
            });
        }

        [HttpPost]
        [EpcAuth(Permission = "manage.menu.edit")]
        public async Task<ActionResult> Save(string data)
        {
            return await RunActionAsync(async () =>
            {
                var menu = data?.JsonToEntity<MenuEntity>(throwIfException: false);
                if (menu == null)
                {
                    return GetJsonRes("参数错误");
                }

                if (ValidateHelper.IsPlumpString(menu.UID))
                {
                    var res = await this._menuService.UpdateMenu(menu);
                    if (res.error)
                    {
                        return GetJsonRes(res.msg);
                    }
                }
                else
                {
                    menu.AsFirstLevelIfParentIsNotValid();
                    var res = await this._menuService.UpdateMenu(menu);
                    if (res.error)
                    {
                        return GetJsonRes(res.msg);
                    }
                }

                return GetJsonRes(string.Empty);
            });
        }

        [HttpPost]
        [EpcAuth(Permission = "manage.menu.delete")]
        public async Task<ActionResult> Delete(string uid)
        {
            return await RunActionAsync(async () =>
            {
                var res = await this._menuService.DeleteMenuWhenNoChildren(uid);
                if (res.error)
                {
                    return GetJsonRes(res.msg);
                }

                return GetJsonRes(string.Empty);
            });
        }
    }
}