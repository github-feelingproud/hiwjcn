using Autofac;
using Lib.ioc;

namespace Hiwjcn.Web.App_Start
{
    public class FullDependencyRegistrar : DependencyRegistrarBase
    {
        public override void Register(ref ContainerBuilder builder)
        {
            //注册控制器
            //RegController(ref builder);
            //builder.RegisterControllers(tps.web.Assembly);
            var pluginAssemblies = FindPluginAssemblies();
            pluginAssemblies.Add(this.GetType().Assembly);
            RegController(ref builder, pluginAssemblies.ToArray());
        }
    }
}