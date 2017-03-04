using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lib;
using Lib.core;
using Lib.helper;
using Lib.io;
using Model;
using Model.User;
using Bll.User;
using Lib.http;
using Lib.helper;
using WebCore.MvcLib.Controller;
using WebCore.MvcLib;
using WebLogic.Model.User;
using Hiwjcn.Core.Infrastructure.User;

namespace WebApp.Areas.Admin.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [ValidateInput(false)]
    public class UserController : WebCore.MvcLib.Controller.UserBaseController
    {
        private IUserService _IUserService { get; set; }
        public UserController(IUserService user)
        {
            this._IUserService = user;
        }
        /// <summary>
        /// 用户列表
        /// </summary>
        /// <returns></returns>
        public ActionResult UserList(int? page, string q)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                page = Com.CheckPage(page);
                int pageSize = 16;
                ViewData["q"] = q;
                var data = _IUserService.GetPagerList(keywords: q, page: page.Value, pageSize: pageSize,
                    LoadRoleAndPrivilege: true);
                if (data != null)
                {
                    ViewData["list"] = data.DataList;

                    ViewData["pager"] = data.GetPagerHtml("/admin/user/userlist/?", "page", page.Value, pageSize);
                }
                return View();
            });
        }
    }
}
