using Autofac;
using Autofac.Extras.DynamicProxy;
using Autofac.Integration.Mvc;
using Hiwjcn.Bll;
using Hiwjcn.Dal;
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
using WebCore.MvcLib.Controller;

namespace Hiwjcn.Web.App_Start
{
    public class FullDependencyRegistrar : DependencyRegistrarBase
    {
        public override void Register(ref ContainerBuilder builder)
        {
            var tps = new
            {
                web = this.GetType(),
                framework = typeof(UserBaseController),
                service = typeof(MyService),
                core = typeof(EntityDB)
            };

            //注册控制器
            builder.RegisterControllers(tps.web.Assembly);
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

            #region 任务调度
            //自动注册调度任务
            RegTasks(ref builder, tps.framework.Assembly);
            #endregion

            #region 注册Data
            //注册数据访问层
            RegDataRepository(ref builder, tps.core.Assembly);
            #endregion

            #region 注册service
            //逻辑代码注册
            RegService(ref builder, true, tps.service.Assembly);
            #endregion

            #region 注册事件
            //事件注册
            RegEvent(ref builder, tps.service.Assembly);
            #endregion
        }
    }
}