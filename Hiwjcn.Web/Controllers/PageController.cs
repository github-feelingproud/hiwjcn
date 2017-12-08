using Hiwjcn.Framework;
using Lib.cache;
using Lib.helper;
using Lib.mvc;
using Lib.mvc.attr;
using System;
using System.Web.Mvc;

namespace Hiwjcn.Web.Controllers
{
    public class PageController : BaseController
    {
        private readonly ICacheProvider _cache;

        /// <summary>
        /// 构造器
        /// </summary>
        public PageController(
            ICacheProvider _cache)
        {
            this._cache = _cache;
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        [HtmlCache(3)]
        [RequestLog]
        public ActionResult Home()
        {
            return RunAction(() =>
            {
                return View();
            });
        }

        [RequestLog]
        [Route("Page/Show/{name}/")]
        public ActionResult Show(string name)
        {
            return RunAction(() =>
            {
                return View();
            });
        }

        /// <summary>
        /// 新闻
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [RequestLog]
        public ActionResult News(int? page)
        {
            return RunAction(() =>
            {
                return View();
            });
        }

        /// <summary>
        /// 保持唤醒的请求地址
        /// </summary>
        /// <returns></returns>
        public ActionResult KeepAlive()
        {
            var alive_key = "alive";
            Session[alive_key] = DateTime.Now.ToString();
            return Content(ConvertHelper.GetString(Session[alive_key]));
        }

    }
}
