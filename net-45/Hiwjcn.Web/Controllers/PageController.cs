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
        
        public PageController(
            ICacheProvider _cache)
        {
            this._cache = _cache;
        }
        
        [RequestLog]
        public ActionResult Home() => Content(nameof(Home));

        [HtmlCache(3)]
        public ActionResult Page() => Content(nameof(Page));
    }
}
