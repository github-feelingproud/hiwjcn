using Hiwjcn.Core.Infrastructure.Common;
using Hiwjcn.Framework.Tasks;
using Lib.helper;
using Model.Sys;
using System.Web.Mvc;

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
