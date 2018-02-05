using Autofac;
using Autofac.Extras.DynamicProxy;
using Autofac.Integration.Mvc;
using Castle.DynamicProxy;
using Lib.data;
using Lib.data.ef;
using Lib.data.mongodb;
using Lib.events;
using Lib.extension;
using Lib.helper;
using Lib.infrastructure;
using Lib.mvc.plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Lib.ioc
{
    /// <summary>
    /// 注册IOC依赖
    /// 搜索“autofac泛型注入”
    /// </summary>
    public abstract class DependencyRegistrarBase : IDependencyRegistrar
    {
        public abstract void Register(ref ContainerBuilder builder);

        private readonly IDictionary<Assembly, List<Type>> _cache = new Dictionary<Assembly, List<Type>>();

        /// <summary>
        /// 缓存需要注册的类，排除泛型
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        private List<Type> CachedClass(Assembly a)
        {
            if (!this._cache.ContainsKey(a))
            {
                this._cache[a] = a.FindAllRegistableClass().Where(x => !x.IsGenericType).ToList();
            }

            return this._cache[a] ?? throw new Exception("无法获取可以注册的类");
        }

        /// <summary>
        /// 自动注册
        /// </summary>
        protected virtual void AutoRegistered(ref ContainerBuilder builder, params Assembly[] ass)
        {
            foreach (var a in ass)
            {
                var types = this.CachedClass(a).Where(x => x.IsAssignableTo_<IAutoRegistered>()).ToArray();
                foreach (var t in types)
                {
                    var reg = builder.RegisterType(t).AsSelf().AsImplementedInterfaces();
                    if (t.IsSingleInstance())
                    {
                        reg.SingleInstance();
                    }

                    if (t.IsInterceptClass())
                    {
                        reg = reg.EnableClassInterceptors();
                    }
                }
            }
        }

        /// <summary>
        /// 注册所有实现了仓储的类
        /// </summary>
        protected virtual void RegDataRepository_(ref ContainerBuilder builder, params Assembly[] ass)
        {
            foreach (var a in ass)
            {
                foreach (var t in this.CachedClass(a))
                {
                    var all_interfaces = t.GetAllInterfaces_().ToArray();
                    if (all_interfaces.Any(x => x.IsGenericType_(typeof(IRepository<>))))
                    {
                        var not_generic_type = all_interfaces.Where(x => !x.IsGenericType).ToArray();
                        var reg = builder.RegisterType(t).AsSelf().As(not_generic_type);
                    }
                }
            }
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        protected virtual void RegService_(ref ContainerBuilder builder, params Assembly[] ass)
        {
            foreach (var a in ass)
            {
                foreach (var t in this.CachedClass(a))
                {
                    var all_interfaces = t.GetAllInterfaces_().ToArray();
                    if (all_interfaces.Any(x => x.IsGenericType_(typeof(IServiceBase<>))))
                    {
                        var not_generic_type = all_interfaces.Where(x => !x.IsGenericType).ToArray();
                        var reg = builder.RegisterType(t).AsSelf().As(not_generic_type);
                        if (t.IsInterceptClass())
                        {
                            reg = reg.EnableClassInterceptors();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 注册aop拦截类
        /// </summary>
        protected virtual void RegAop(ref ContainerBuilder builder, params Assembly[] ass)
        {
            foreach (var a in ass)
            {
                var tps = this.CachedClass(a).Where(x => x.IsAssignableTo_<IInterceptor>()).ToArray();
                builder.RegisterTypes(tps);
            }
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        protected virtual void RegEvent(ref ContainerBuilder builder, params Assembly[] ass)
        {
            foreach (var a in ass)
            {
                try
                {
                    foreach (var t in this.CachedClass(a))
                    {
                        var interfaces = t.GetInterfaces().Where(x => x.IsGenericType_(typeof(IConsumer<>))).ToArray();
                        if (interfaces?.Count() > 0)
                        {
                            //注册到所有接口
                            builder.RegisterType(t).AsSelf().As(interfaces);
                        }
                    }
                }
                catch (Exception e)
                {
                    //Entity Framework 6不允许get types，抛了一个异常
                    e.AddLog("注册事件发布异常");
                    continue;
                }
            }
            builder.RegisterType<EventPublisher>().AsSelf().As<IEventPublisher>().SingleInstance();
        }

        /// <summary>
        /// 获取插件程序集(还需完善)
        /// </summary>
        protected virtual List<Assembly> FindPluginAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("Hiwjcn.Plugin.")).ToList();
        }

        /// <summary>
        /// 注册插件的控制器
        /// </summary>
        protected virtual void RegController(ref ContainerBuilder builder, params Assembly[] ass)
        {
            foreach (var a in ass)
            {
                //注册URL
                builder.RegisterControllers(a);
                //注册插件
                var tps = this.CachedClass(a).Where(x => x.IsAssignableTo_<BasePaymentController>()).ToArray();
                if (ValidateHelper.IsPlumpList(tps))
                {
                    builder.RegisterTypes(tps).As<BasePaymentController>();
                }
            }
        }

        public void Clean()
        {
            this._cache.Clear();
        }
    }
}
