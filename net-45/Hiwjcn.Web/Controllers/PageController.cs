using Hiwjcn.Framework;
using Lib.cache;
using Lib.helper;
using Lib.mvc;
using Lib.mvc.attr;
using System;
using System.Web.Mvc;

namespace Hiwjcn.Web.Controllers
{
    public class PageController : EpcBaseController
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
        public ActionResult Home() => Content(string.Empty);
    }
}
