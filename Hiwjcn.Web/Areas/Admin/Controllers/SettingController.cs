using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lib.core;
using Lib;
using Model;
using Model.Sys;
using Lib.http;
using Lib.helper;
using Bll;
using Bll.Sys;
using Model.User;
using WebCore.MvcLib.Controller;
using WebCore.MvcLib;
using Hiwjcn.Framework.Tasks;
using Hiwjcn.Core.Infrastructure.Common;

namespace WebApp.Areas.Admin.Controllers
{
    public class SettingController : WebCore.MvcLib.Controller.UserBaseController
    {
        private ISettingService _ISettingService { get; set; }
        public SettingController(ISettingService setting)
        {
            this._ISettingService = setting;
        }

        /// <summary>
        /// 后台服务
        /// </summary>
        /// <returns></returns>
        public ActionResult ServiceTask()
        {
            return RunActionWhenLogin((loginuser) =>
            {
                ViewData["list"] = TaskManager.GetAllTasks();
                return View();
            });
        }

        /// <summary>
        /// 设置处理请求
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveSettingAction(string key, string value)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var model = new OptionModel();
                model.Key = key;
                model.Value = value;

                return GetJsonRes(_ISettingService.SaveOption(model));
            });
        }

        /// <summary>
        /// 设置
        /// </summary>
        /// <returns></returns>
        public ActionResult Setting()
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var list = _ISettingService.GetAllOptions();
                if (ValidateHelper.IsPlumpList(list))
                {
                    foreach (var model in list)
                    {
                        ViewData[model.Key] = model.Value;
                    }
                }
                return View();
            });
        }
    }
}
