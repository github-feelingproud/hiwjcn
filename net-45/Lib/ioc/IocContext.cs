using Autofac;
using Lib.core;
using Lib.extension;
using Lib.helper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Lib.ioc
{
    public class IocContext : IDisposable
    {
        public static readonly IocContext Instance = new IocContext();

        private IServiceProvider _root;

        public IocContext() { }

        public IocContext SetRootContainer(IServiceProvider root)
        {
            this._root = root;
            return this;
        }

        public IServiceProvider Container => this._root ?? throw new Exception("没有设置依赖注入容器");

        public IServiceScope Scope() => this.Container.CreateScope();

        public bool IsRegistered<T>()
        {
            using (var s = this.Scope())
            {
                return s.ServiceProvider.GetServices<T>().Any();
            }
        }

        public void Dispose()
        {
            //
        }
    }

    /// <summary>
    /// IOC容器
    /// https://autofac.org/
    /// http://autofac.readthedocs.io/en/latest/getting-started/index.html
    /// </summary>
    public class AutofacIocContext : IDisposable
    {
        /// <summary>
        /// 默认实例
        /// </summary>
        public static readonly AutofacIocContext Instance = new AutofacIocContext();

        private readonly Lazy_<IContainer> _lazy;

        /// <summary>
        /// 创建之前调用
        /// </summary>
        public event RefAction<ContainerBuilder> OnContainerBuilding;

        /// <summary>
        /// 再更高层添加数据
        /// </summary>
        private readonly List<IDependencyRegistrar> ExtraRegistrars = new List<IDependencyRegistrar>();

        public AutofacIocContext()
        {
            this._lazy = new Lazy_<IContainer>(() =>
            {
                //创建builder
                var builder = new ContainerBuilder();
                //注册依赖
                new BaseDependencyRegistrar().Register(ref builder);
                //注册额外依赖
                if (ValidateHelper.IsPlumpList(this.ExtraRegistrars))
                {
                    foreach (var reg in this.ExtraRegistrars)
                    {
                        reg.Register(ref builder);
                        reg.Clean();
                    }
                }

                //额外的切入点
                this.OnContainerBuilding?.Invoke(ref builder);

                //创建容器
                var context = builder.Build();

                return context;
            }).WhenDispose((ref IContainer x) => x.Dispose());
        }

        /// <summary>
        /// 添加额外的注册（这个操作要尽量早执行）
        /// </summary>
        /// <param name="reg"></param>
        public AutofacIocContext AddExtraRegistrar(IDependencyRegistrar reg)
        {
            if (this._lazy.IsValueCreated)
            {
                throw new Exception("依赖注入容器已经生成，请在生成前注册额外依赖");
            }
            this.ExtraRegistrars.Add(reg);
            return this;
        }

        /// <summary>
        /// 销毁容器
        /// </summary>
        public void Dispose()
        {
            this._lazy.Dispose();
        }

        /// <summary>
        /// 获取ioc容器，第一次访问将创建容器
        /// </summary>
        /// <returns></returns>
        public IContainer Container
        {
            get => this._lazy.Value;
        }

        /// <summary>
        /// 是否在容器中注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsRegistered<T>() => this.Container.IsRegistered<T>();

        /// <summary>
        /// 是否在容器中注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsRegisteredWithName<T>(string name) => this.Container.IsRegisteredWithName<T>(name);

        /// <summary>
        /// 是否在容器中注册
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsRegistered<T>(string name)
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
        /// 创建一个作用域
        /// </summary>
        /// <returns></returns>
        public ILifetimeScope Scope() => this.Container.BeginLifetimeScope();

        /// <summary>
        /// 生命周期
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public T Scope<T>(Func<ILifetimeScope, T> func)
        {
            using (var scope = Scope())
            {
                return func.Invoke(scope);
            }
        }

        /// <summary>
        /// 生命周期
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public async Task<T> ScopeAsync<T>(Func<ILifetimeScope, Task<T>> func)
        {
            using (var scope = Scope())
            {
                return await func.Invoke(scope);
            }
        }
    }

    [Obsolete("将被中间件替代")]
    public class RequestScopeModule : IHttpModule
    {
        public void Dispose()
        {
            $"{nameof(RequestScopeModule)}被销毁".AddBusinessInfoLog();
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += (sender, e) =>
            {
                HttpContext.Current.SetAutofacScope(AutofacIocContext.Instance.Scope());
            };

            context.EndRequest += (sender, e) =>
            {
                try
                {
                    var scope = HttpContext.Current.GetAutofacScope();
                    scope.Dispose();
                }
                catch (Exception err)
                {
                    err.AddErrorLog("销毁请求scope失败");
                }
            };
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
