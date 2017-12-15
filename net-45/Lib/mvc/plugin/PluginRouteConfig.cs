using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Lib.core;
using Lib.extension;
using Lib.helper;

namespace Lib.mvc.plugin
{
    public static class PluginRouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            if (!ConfigHelper.Instance.LoadPlugin) { return; }
            //注册插件路由
            var routelist = new List<IRouteProvider>();
            var ass = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.IndexOf("Plugin") >= 0).ToList();
            foreach (var a in ass)
            {
                var tps = a.GetTypes().Where(x => x.IsNormalClass() && x.IsAssignableTo_<IRouteProvider>()).ToList();
                if (!ValidateHelper.IsPlumpList(tps))
                {
                    continue;
                }
                if (tps.Count != 1)
                {
                    throw new Exception("每个插件中只能有一个路由配置");
                }

                var r = tps.FirstOrDefault();

                var router = (IRouteProvider)Activator.CreateInstance(r);
                routelist.Add(router);
            }
            //按照优先级注册路由
            routelist.OrderByDescending(x => x.Priority).ToList().ForEach(x =>
            {
                x.RegisterRoutes(routes);
            });
        }
    }
}