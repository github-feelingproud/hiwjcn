using Autofac;
using Autofac.Extras.DynamicProxy;
using Lib.cache;
using Lib.data;
using Lib.events;
using Lib.extension;
using Lib.helper;
using Lib.infrastructure;
using Lib.ioc;
using Lib.task;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Lib.ioc
{
    /// <summary>
    /// 注册IOC依赖
    /// </summary>
    public abstract class DependencyRegistrarBase : IDependencyRegistrar
    {
        public abstract void Register(ref ContainerBuilder builder);

        /// <summary>
        /// 注册任务
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="ass"></param>
        protected void RegTasks(ref ContainerBuilder builder, params Assembly[] ass)
        {
            foreach (var a in ass)
            {
                var jobTypes = a.GetTypes().ToArray();
                jobTypes = jobTypes.Where(x => x.IsAssignableTo<QuartzJobBase>()
                && !x.IsAbstract
                && !x.IsInterface).ToArray();
                if (ValidateHelper.IsPlumpList(jobTypes))
                {
                    builder.RegisterTypes(jobTypes).As<QuartzJobBase>();
                }
            }
        }

        /// <summary>
        /// 注册仓库
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="ass"></param>
        protected void RegDataRepository(ref ContainerBuilder builder, params Assembly[] ass)
        {
            foreach (var a in ass)
            {
                foreach (var t in a.GetTypes())
                {
                    if (t.BaseType != null &&
                        t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(EFRepository<>))
                    {
                        var interfaces = t.BaseType.GetInterfaces().Where(x =>
                        x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRepository<>));

                        if (interfaces?.Count() > 0)
                        {
                            builder.RegisterType(t).As(interfaces.ToArray());
                        }
                        else
                        {
                            builder.RegisterType(t).As(t);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="intercept"></param>
        /// <param name="ass"></param>
        protected void RegService(ref ContainerBuilder builder, bool intercept, params Assembly[] ass)
        {
            foreach (var a in ass)
            {
                foreach (var t in a.GetTypes())
                {
                    //注册service
                    if (t.BaseType != null &&
                        t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(ServiceBase<>))
                    {
                        var interfaces = t.GetInterfaces().Where(x =>
                        x.GetInterfaces().Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IServiceBase<>)));
                        if (interfaces?.Count() > 0)
                        {
                            if (intercept)
                            {
                                builder.RegisterType(t).As(interfaces.ToArray()).EnableClassInterceptors();
                            }
                            else
                            {
                                builder.RegisterType(t).As(interfaces.ToArray());
                            }
                        }
                        else
                        {
                            if (intercept)
                            {
                                builder.RegisterType(t).As(t).EnableClassInterceptors();
                            }
                            else
                            {
                                builder.RegisterType(t).As(t);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="ass"></param>
        protected void RegEvent(ref ContainerBuilder builder, params Assembly[] ass)
        {
            var consumerType = typeof(IConsumer<>);
            foreach (var a in ass)
            {
                try
                {
                    //找到包含consumer的类
                    var types = a.GetTypes().Where(x =>
                    x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == consumerType));
                    foreach (var t in types)
                    {
                        //找到接口
                        var interfaces = t.GetInterfaces().Where(x =>
                        x.IsGenericType && x.GetGenericTypeDefinition() == consumerType).ToArray();
                        //注册到所有接口
                        builder.RegisterType(t).As(interfaces);
                    }
                }
                catch (Exception e)
                {
                    //Entity Framework 6不允许get types，抛了一个异常
                    e.SaveLog("注册事件发布异常");
                    continue;
                }
            }
            builder.RegisterType<EventPublisher>().As<IEventPublisher>().SingleInstance();
        }

        /// <summary>
        /// 注册插件的控制器
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="ass"></param>
        protected void RegPlguinController(ref ContainerBuilder builder, params Assembly[] ass)
        {
            throw new NotImplementedException();
        }

    }
}
