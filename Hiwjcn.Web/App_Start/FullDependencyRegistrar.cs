using Autofac;
using Autofac.Extras.DynamicProxy;
using Autofac.Integration.Mvc;
using Bll;
using Dal;
using Hiwjcn.Bll;
using Hiwjcn.Dal;
using Lib.cache;
using Lib.data;
using Lib.events;
using Lib.extension;
using Lib.infrastructure;
using Lib.ioc;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Hiwjcn.Web.App_Start
{
    public class FullDependencyRegistrar : IDependencyRegistrar
    {
        public void Register(ref ContainerBuilder builder)
        {
            //注册控制器
            builder.RegisterControllers(this.GetType().Assembly);
            //Aop拦截
            builder.RegisterType<AopLogError>();
            //缓存
            var UseRedis = false;
            if (UseRedis)
            {
                builder.RegisterType<RedisCacheProvider>().As<ICacheProvider>().SingleInstance();
            }
            else
            {
                builder.RegisterType<MemoryCacheProvider>().As<ICacheProvider>().SingleInstance();
            }
            builder.RegisterType<EntityDB>().Named<DbContext>("db");
            builder.RegisterType<MySqlConnection>().As<IDbConnection>();

            #region 注册Data
            //注册数据访问层
            foreach (var t in typeof(EntityDB).Assembly.GetTypes())
            {
                if (t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(EFRepository<>))
                {
                    var interfaces = t.BaseType.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRepository<>));
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
            #endregion

            #region 注册service
            var serviceAss = typeof(MyService).Assembly;
            //注册service
            foreach (var t in serviceAss.GetTypes())
            {
                //注册service
                if (t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == typeof(ServiceBase<>))
                {
                    var interfaces = t.GetInterfaces().Where(x => x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IServiceBase<>)));
                    if (interfaces?.Count() > 0)
                    {
                        builder.RegisterType(t).As(interfaces.ToArray()).EnableClassInterceptors();
                    }
                    else
                    {
                        builder.RegisterType(t).As(t).EnableClassInterceptors();
                    }
                }
            }
            #endregion

            #region 注册事件
            var ass = new Assembly[] { serviceAss };
            //事件注册
            var consumerType = typeof(IConsumer<>);
            foreach (var a in ass)
            {
                try
                {
                    //找到包含consumer的类
                    var types = a.GetTypes().Where(x => x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == consumerType));
                    foreach (var t in types)
                    {
                        //找到接口
                        var interfaces = t.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == consumerType).ToArray();
                        //注册到所有接口
                        builder.RegisterType(t).As(interfaces).InstancePerLifetimeScope();
                    }
                }
                catch (Exception e)
                {
                    //Entity Framework 6不允许get types，抛了一个异常
                    e.AddLog("注册事件发布异常");
                    continue;
                }
            }
            builder.RegisterType<EventPublisher>().As<IEventPublisher>().SingleInstance();
            #endregion
        }
    }
}