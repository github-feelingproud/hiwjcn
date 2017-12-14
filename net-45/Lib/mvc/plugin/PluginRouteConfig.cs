using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Lib.core;

namespace Lib.mvc.plugin
{
    public class PluginRouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            if (!ConfigHelper.Instance.LoadPlugin) { return; }
            //注册插件路由
            var routelist = new List<IRouteProvider>();
            var ass = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.IndexOf("Plugin") >= 0).ToList();
            foreach (var a in ass)
            {
                var r = a.GetTypes().Where(x => x.GetInterfaces().FirstOrDefault() == typeof(IRouteProvider)).FirstOrDefault();
                if (r != null)
                {
                    var router = (IRouteProvider)a.CreateInstance(r.FullName);
                    routelist.Add(router);
                }
            }
            //按照优先级注册路由
            routelist.OrderByDescending(x => x.Priority).ToList().ForEach(x =>
            {
                x.RegisterRoutes(routes);
            });
        }
    }
}