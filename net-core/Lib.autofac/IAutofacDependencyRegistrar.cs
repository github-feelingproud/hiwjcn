using Autofac;

namespace Lib.ioc
{
    /// <summary>
    /// 依赖注册接口
    /// </summary>
    public interface IAutofacDependencyRegistrar
    {
        void Register(ref ContainerBuilder builder);

        void Clean();
    }
}
