using Bll.Product;
using Dal.Post;
using Dal.Product;
using Dal.User;
using Hiwjcn.Core.Infrastructure.User;
using Hiwjcn.Dal.Sys;
using Lib.helper;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace WebApp.Areas.Admin.Controllers
{
    /// <summary>
    /// 用作引导页
    /// </summary>
    public class PanelController : WebCore.MvcLib.Controller.UserController
    {
        private IUserService _IUserService { get; set; }
        public PanelController(IUserService user)
        {
            this._IUserService = user;
        }
        /// <summary>
        /// 统计页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Welcome()
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var udal = new UserDal();
                ViewData["c1"] = udal.GetCount(null);

                var pdal = new PostDal();
                ViewData["c2"] = pdal.GetCount(null);

                var prodal = new ProductDal();
                ViewData["c3"] = prodal.GetCount(null);

                var odal = new OrderDal();
                ViewData["c4"] = odal.GetCount(null);

                var now = DateTime.Now;
                var reqdal = new ReqLogDal();
                reqdal.PrepareIQueryable((query) =>
                {
                    query = query.Where(x => x.UpdateTime < now).OrderByDescending(x => x.UpdateTime).Skip(0).Take(1000);
                    var avgreqtime = query.Average(x => x.ReqTime);
                    ViewData["avgreqtime"] = avgreqtime ?? 0;
                    return true;
                });

                /////////////////////////////////////////////

                var userdata = _IUserService.GetCountGroupBySex();
                if (!ValidateHelper.IsPlumpList(userdata))
                {
                    return GetJson(new { success = false });
                }
                ViewData["user_data"] = JsonHelper.ObjectToJson(new
                {
                    success = true,
                    legend_data = userdata.Select(x => x.Sex).ToArray(),
                    data = userdata.Select(x => new { value = x.Count, name = x.Sex })
                });

                /////////////////////////////////////////////

                var orderbll = new OrderBll();
                var orderdata = orderbll.GetOrderCountGroupByState();
                if (!ValidateHelper.IsPlumpList(orderdata))
                {
                    return GetJson(new { success = false });
                }
                ViewData["order_data"] = JsonHelper.ObjectToJson(new
                {
                    success = true,
                    legend_data = orderdata.Select(x => x.StateName).ToArray(),
                    data = orderdata.Select(x => new { value = x.Count, name = x.StateName })
                });
                return View();
            });
        }

        /// <summary>
        /// 读取磁盘空间
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DiskUsageCountAction()
        {
            return RunActionWhenLogin((loginuser) =>
            {
                DriveInfo[] drivers = DriveInfo.GetDrives();
                if (!ValidateHelper.IsPlumpList(drivers))
                {
                    return GetJson(new { success = false, proportion = 0 });
                }

                //计算比例
                var proportion = drivers.Sum(x => x.TotalFreeSpace) / drivers.Sum(x => x.TotalSize);

                return GetJson(new { success = true, proportion = proportion });
            });
        }

    }
}
