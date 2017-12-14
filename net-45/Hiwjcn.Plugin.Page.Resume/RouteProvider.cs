using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;
using Lib.mvc.plugin;

namespace Hiwjcn.Plugin.Page.Resume
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority
        {
            get
            {
                return 0;
            }
        }

        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Hiwjcn.Plugin.Page.Resume",
                 "Plugins/Page/Resume",
                 new { controller = "Resume", action = "Show" },
                 new[] { "Hiwjcn.Plugin.Page.Resume.Controllers" }
            );
        }
    }
}
