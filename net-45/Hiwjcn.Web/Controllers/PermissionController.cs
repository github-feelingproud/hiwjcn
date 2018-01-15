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
    public class PermissionController : UserBaseController
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
                var iviewdata = list.Select(x => new IViewTreeNode()
                {
                    id = x.UID,
                    pId = x.ParentUID,
                    title = x.Name,
                    expand = false,
                    @checked = false,
                    selected = false,
                }).ToList();

                return GetJson(new _()
                {
                    success = true,
                    data = TreeHelper.BuildIViewTreeStructure(iviewdata)
                });
            });
        }
    }
}