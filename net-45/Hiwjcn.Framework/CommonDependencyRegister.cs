using Autofac;
using EPC.Core;
using Hiwjcn.Core.Data;
using Hiwjcn.Dal;
using Hiwjcn.Service;
using Lib.cache;
using Lib.core;
using Lib.helper;
using Lib.infrastructure;
using Lib.ioc;
using MySql.Data.MySqlClient;
using WebCore.MvcLib.Controller;

namespace Hiwjcn.Framework
{
    public class CommonDependencyRegister : DependencyRegistrarBase
    {
        public override void Register(ref ContainerBuilder builder)
        {
            var tps = new
            {
                framework = typeof(UserBaseController).Assembly,
                service = typeof(MyService).Assembly,
                core = typeof(EntityDB).Assembly
            };

            //Aop拦截
            builder.RegisterType<AopLogError_>();
            //缓存
            if (ValidateHelper.IsPlumpString(ConfigHelper.Instance.RedisConnectionString))
            {
                builder.UseCacheProvider<RedisCacheProvider_>();
            }
            else
            {
                builder.UseCacheProvider<MemoryCacheProvider>();
            }
            builder.UseSystemConfig<BasicConfigProvider>();
            builder.UseEF<EntityDB>("db");
            builder.UseAdoConnection(() =>
            {
                var con = new MySqlConnection("xxxx");
                con.Open();
                return con;
            });
            //builder.RegisterInstance(new LoginStatus()).As<LoginStatus>().SingleInstance();
            //builder.Register(_ => new LoginStatus("hiwjcn_uid", "hiwjcn_token", "hiwjcn_login_session", "")).AsSelf().SingleInstance();

            #region 自动注册
            AutoRegistered(ref builder, tps.core, tps.service, tps.framework);
            #endregion

            #region 注册Data
            //注册数据访问层
            RegDataRepository_(ref builder, tps.core);
            builder.RegisterGeneric(typeof(MemberShipRepository<>)).As(typeof(IMSRepository<>));
            builder.RegisterGeneric(typeof(EpcRepository<>)).As(typeof(IEpcRepository<>));
            //builder.RegisterGeneric(typeof(MongoRepository<>)).AsSelf().As(typeof(IMongoRepository<>));
            #endregion

            #region 注册service
            //逻辑代码注册
            RegService_(ref builder, tps.service);
            builder.RegisterGeneric(typeof(ServiceBase<>)).AsSelf().As(typeof(IServiceBase<>));
            #endregion

            #region 注册事件
            //事件注册
            RegEvent(ref builder, tps.service);
            #endregion
        }
    }
}
