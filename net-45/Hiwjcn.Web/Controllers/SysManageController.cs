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
using Hiwjcn.Bll;

namespace Hiwjcn.Web.Controllers
{
    public class SysManageController : UserBaseController
    {
        private readonly ISystemService _sysService;

        public SysManageController(
            ISystemService _sysService)
        {
            this._sysService = _sysService;
        }

        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> Query()
        {
            return await RunActionAsync(async () =>
            {
                var data = await this._sysService.QueryAll();
                return GetJson(new _()
                {
                    success = true,
                    data = data
                });
            });
        }
    }
}