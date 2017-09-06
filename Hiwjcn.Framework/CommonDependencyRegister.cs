using Autofac;
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
using Bll.User;
using Hiwjcn.Bll.Auth;
using Lib.mvc.auth;

namespace Hiwjcn.Framework
{
    public class CommonDependencyRegister : DependencyRegistrarBase
    {
        public override bool Intercept => true;

        public override void Register(ref ContainerBuilder builder)
        {
            var tps = new
            {
                framework = typeof(UserBaseController),
                service = typeof(MyService),
                core = typeof(EntityDB)
            };

            //Aop拦截
            builder.RegisterType<AopLogError_>();
            //缓存
            if (ValidateHelper.IsPlumpString(ConfigHelper.Instance.RedisConnectionString))
            {
                builder.UseCacheProvider<RedisCacheProvider>();
            }
            else
            {
                builder.UseCacheProvider<MemoryCacheProvider>();
            }
            builder.UseSystemConfig<BasicConfigProvider>();
            builder.UseEF<EntityDB>("db");
            builder.UseAdoConnection<MySqlConnection>();
            //builder.RegisterInstance(new LoginStatus()).As<LoginStatus>().SingleInstance();
            //builder.Register(_ => new LoginStatus("hiwjcn_uid", "hiwjcn_token", "hiwjcn_login_session", "")).AsSelf().As<ILoginStatus>().SingleInstance();

            #region 自动注册
            AutoRegistered(ref builder, tps.core.Assembly, tps.service.Assembly, tps.framework.Assembly);
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
