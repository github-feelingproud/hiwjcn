using Autofac;
using Autofac.Integration.Mvc;
using Hiwjcn.Bll;
using Hiwjcn.Dal;
using Lib.cache;
using Lib.ioc;
using Lib.core;
using Lib.mvc.user;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Entity;
using WebCore.MvcLib.Controller;
using Lib.helper;
using System;

namespace Hiwjcn.Web.App_Start
{
    public class FullDependencyRegistrar : DependencyRegistrarBase
    {
        public override bool Intercept => true;

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
            //RegController(ref builder);
            //builder.RegisterControllers(tps.web.Assembly);
            var pluginAssemblies = FindPluginAssemblies();
            pluginAssemblies.Add(tps.web.Assembly);
            RegController(ref builder, pluginAssemblies.ToArray());
            //Aop拦截
            builder.RegisterType<AopLogError_>();
            //缓存
            if (ValidateHelper.IsPlumpString(ConfigHelper.Instance.RedisConnectionString))
            {
                builder.RegisterType<RedisCacheProvider>().As<ICacheProvider>().SingleInstance();
            }
            else
            {
                builder.RegisterType<MemoryCacheProvider>().As<ICacheProvider>().SingleInstance();
            }
            builder.RegisterType<BasicConfigProvider>().As<ISettings>().SingleInstance();
            builder.RegisterType<EntityDB>().Named<DbContext>("db");
            builder.RegisterType<MySqlConnection>().As<IDbConnection>();
            builder.RegisterType<GetLocalLoginUrl>().As<IGetLoginUrl>().SingleInstance();
            //builder.RegisterInstance(new LoginStatus()).As<LoginStatus>().SingleInstance();
            builder.Register(_ => new LoginStatus()).As<LoginStatus>().As<ILoginStatus>().SingleInstance();

            #region 自动注册
            AutoRegistered(ref builder, tps.core.Assembly, tps.service.Assembly);
            #endregion

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
            RegService(ref builder, tps.service.Assembly);
            #endregion

            #region 注册事件
            //事件注册
            RegEvent(ref builder, tps.service.Assembly);
            #endregion
        }
    }
}