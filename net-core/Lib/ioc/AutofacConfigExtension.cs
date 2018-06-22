using Autofac;
using Autofac.Builder;
using Autofac.Integration.Mvc;
using Lib.cache;
using Lib.core;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Lib.extension;
using System.Data;
using System.Data.Entity;
using Lib.mq;
using Microsoft.Extensions.DependencyInjection;

namespace Lib.ioc
{
    public static class AutofacConfigExtension
    {
        /// <summary>
        /// 使用EF
        /// </summary>
        public static void UseEF<T>(this ContainerBuilder builder, string name = null)
            where T : DbContext
        {
            var context = builder.RegisterType<T>().AsSelf().As<DbContext>();
            if (ValidateHelper.IsPlumpString(name))
            {
                context = context.Named<DbContext>(name);
            }
        }

        /// <summary>
        /// 使用配置
        /// </summary>
        public static void UseSystemConfig<T>(this ContainerBuilder builder)
            where T : class, ISettings =>
            builder.RegisterType<T>().AsSelf().AsImplementedInterfaces().As<ISettings>().SingleInstance();

        /// <summary>
        /// 使用数据库
        /// </summary>
        public static void UseAdoConnection(this ContainerBuilder builder, Func<IDbConnection> get_opened_connction) =>
            builder.Register(x => get_opened_connction.Invoke()).AsSelf().As<IDbConnection>().InstancePerDependency();

        /// <summary>
        /// 使用缓存
        /// </summary>
        public static void UseCacheProvider<T>(this ContainerBuilder builder)
            where T : class, ICacheProvider =>
            builder.RegisterType<T>().AsSelf().AsImplementedInterfaces().As<ICacheProvider>().SingleInstance();

        /// <summary>
        /// 使用消息队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        public static void UseMessageQueue<T>(this ContainerBuilder builder)
            where T : class, IMessageQueueProducer =>
            builder.RegisterType<T>().AsSelf().AsImplementedInterfaces().As<IMessageQueueProducer>().SingleInstance();

    }
}
