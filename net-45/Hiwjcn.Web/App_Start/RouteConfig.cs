using Lib.mvc.plugin;
using System.Web.Mvc;
using System.Web.Routing;

namespace Hiwjcn.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //开启route特性功能
            routes.MapMvcAttributeRoutes();

            //注册插件路由
            PluginRouteConfig.RegisterRoutes(routes);

            //注册通用路由
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Page", action = "Home", id = UrlParameter.Optional },
                namespaces: new string[]
                {
                    "Hiwjcn.Web.Controllers"
                }
            );
        }
    }
}
