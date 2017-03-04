using Autofac;
using Lib.cache;
using Lib.core;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lib.ioc
{
    /// <summary>
    /// IOC容器
    /// </summary>
    public static class AppContext
    {
        /// <summary>
        /// 再更高层添加数据
        /// </summary>
        private static readonly List<IDependencyRegistrar> ExtraRegistrars = new List<IDependencyRegistrar>();

        /// <summary>
        /// 添加额外的注册（这个操作要尽量早执行）
        /// </summary>
        /// <param name="reg"></param>
        public static void AddExtraRegistrar(IDependencyRegistrar reg)
        {
            if (context != null)
            {
                throw new Exception("依赖注入容器已经生成，请在生成前注册额外依赖");
            }
            ExtraRegistrars.Add(reg);
        }

        /// <summary>
        /// 创建容器线程锁
        /// </summary>
        private static readonly object LOCKER = new object();

        /// <summary>
        /// 容器对象
        /// </summary>
        private static IContainer context = null;

        /// <summary>
        /// 销毁容器
        /// </summary>
        public static void Dispose()
        {
            context?.Dispose();
        }

        /// <summary>
        /// 获取ioc容器，第一次访问将创建容器
        /// </summary>
        /// <returns></returns>
        public static IContainer Container
        {
            get
            {
                if (context == null)
                {
                    lock (LOCKER)
                    {
                        if (context == null)
                        {
                            //创建builder
                            var builder = new ContainerBuilder();
                            //注册依赖
                            new BaseDependencyRegistrar().Register(ref builder);
                            //注册额外依赖
                            if (ValidateHelper.IsPlumpList(ExtraRegistrars))
                            {
                                foreach (var reg in ExtraRegistrars)
                                {
                                    reg.Register(ref builder);
                                }
                            }
                            //创建容器
                            context = builder.Build();
                        }
                    }
                }
                return context;
            }
        }

        /// <summary>
        /// 是否在容器中注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsRegistered<T>() => Container.IsRegistered<T>();

        /// <summary>
        /// 是否在容器中注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsRegisteredWithName<T>(string name) => Container.IsRegisteredWithName<T>(name);

        /// <summary>
        /// 是否在容器中注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsRegistered<T>(string name)
        {
            if (ValidateHelper.IsPlumpString(name))
            {
                return IsRegisteredWithName<T>(name);
            }
            else
            {
                return IsRegistered<T>();
            }
        }
        /// <summary>
        /// 获取对象实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static T GetObject<T>(string name = null) where T : class
        {
            if (ValidateHelper.IsPlumpString(name))
            {
                if (!Container.IsRegisteredWithName<T>(name))
                {
                    throw new Exception($"请注册依赖：{typeof(T)}，name：{name}");
                }
                return Container.ResolveNamed<T>(name);
            }
            else
            {
                if (!Container.IsRegistered<T>())
                {
                    throw new Exception($"请注册依赖：{typeof(T)}");
                }
                return Container.Resolve<T>();
            }
        }
        public static T[] GetAllObject<T>()
        {
            return GetObject<IEnumerable<T>>().ToArray();
        }
    }

    /// <summary>
    /// 依赖注册接口
    /// </summary>
    public interface IDependencyRegistrar
    {
        void Register(ref ContainerBuilder builder);
    }

    /// <summary>
    /// 注册依赖
    /// </summary>
    public class BaseDependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// 注册依赖
        /// </summary>
        /// <param name="builder"></param>
        public void Register(ref ContainerBuilder builder)
        {
            //
        }
    }

    /*
     1、InstancePerDependency

对每一个依赖或每一次调用创建一个新的唯一的实例。这也是默认的创建实例的方式。

官方文档解释：Configure the component so that every dependent component or call to Resolve() gets a new, unique instance (default.)

2、InstancePerLifetimeScope

在一个生命周期域中，每一个依赖或调用创建一个单一的共享的实例，且每一个不同的生命周期域，实例是唯一的，不共享的。

官方文档解释：Configure the component so that every dependent component or call to Resolve() within a single ILifetimeScope gets the same, shared instance. Dependent components in different lifetime scopes will get different instances.

3、InstancePerMatchingLifetimeScope

在一个做标识的生命周期域中，每一个依赖或调用创建一个单一的共享的实例。打了标识了的生命周期域中的子标识域中可以共享父级域中的实例。若在整个继承层次中没有找到打标识的生命周期域，则会抛出异常：DependencyResolutionException。

官方文档解释：Configure the component so that every dependent component or call to Resolve() within a ILifetimeScope tagged with any of the provided tags value gets the same, shared instance. Dependent components in lifetime scopes that are children of the tagged scope will share the parent's instance. If no appropriately tagged scope can be found in the hierarchy an DependencyResolutionException is thrown.

4、InstancePerOwned

在一个生命周期域中所拥有的实例创建的生命周期中，每一个依赖组件或调用Resolve()方法创建一个单一的共享的实例，并且子生命周期域共享父生命周期域中的实例。若在继承层级中没有发现合适的拥有子实例的生命周期域，则抛出异常：DependencyResolutionException。

官方文档解释：

Configure the component so that every dependent component or call to Resolve() within a ILifetimeScope created by an owned instance gets the same, shared instance. Dependent components in lifetime scopes that are children of the owned instance scope will share the parent's instance. If no appropriate owned instance scope can be found in the hierarchy an DependencyResolutionException is thrown.

5、SingleInstance

每一次依赖组件或调用Resolve()方法都会得到一个相同的共享的实例。其实就是单例模式。

官方文档解释：Configure the component so that every dependent component or call to Resolve() gets the same, shared instance.

6、InstancePerHttpRequest

在一次Http请求上下文中,共享一个组件实例。仅适用于asp.net mvc开发。
     */
}
