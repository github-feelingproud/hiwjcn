using Lib.extension;
using Lib.helper;
using Lib.ioc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Autofac;

namespace Lib.data.ef
{
    /// <summary>
    /// EF的帮助类
    /// </summary>
    public partial class EFManager
    {
        private readonly string db_name;
        public EFManager(string name)
        {
            this.db_name = name;
        }
        
        /// <summary>
        /// 获取实体对象
        /// </summary>
        /// <returns></returns>
        public DbContext GetDbContext(ILifetimeScope scope, string db = null)
        {
            /* 提升性能，不要检查
            if (!AppContext.IsRegistered<DbContext>(db_name))
            {
                throw new Exception("请在容器中注册dbcontext实例");
            }*/
            return scope.Resolve_<DbContext>(name: db ?? db_name);
        }

        /// <summary>
        /// 准备实体容器对象
        /// </summary>
        public void PrepareSession(Action<DbContext> callback)
        {
            using (var s = AutofacIocContext.Instance.Scope())
            {
                using (var db = GetDbContext(s))
                {
                    //执行回调
                    callback.Invoke(db);
                }
            }
        }

        public async Task PrepareSessionAsync(Func<DbContext, Task> callback)
        {
            await AutofacIocContext.Instance.ScopeAsync(async x =>
            {
                using (var db = GetDbContext(x))
                {
                    //执行回调
                    await callback.Invoke(db);
                }
                return true;
            });
        }
    }

    partial class EFManager
    {
        /// <summary>
        /// 取消EF首次访问数据库的System.Data.Entity.CreateDatabaseIfNotExists策略
        /// </summary>
        public static void SetNoInitializer<T>() where T : DbContext, new()
        {
            Database.SetInitializer<T>(new NullDatabaseInitializer<T>());
        }

        public static void FastStart<T>() where T : DbContext, new()
        {
            SetNoInitializer<T>();
            using (var db = new T())
            {
                var objectContext = ((IObjectContextAdapter)db).ObjectContext;
                var mappingCollection = (StorageMappingItemCollection)objectContext.MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);
                mappingCollection.GenerateViews(new List<EdmSchemaError>());
            }
        }

        /// <summary>
        /// 尝试创建数据表
        /// </summary>
        public static void TryInstallDatabase<T>() where T : DbContext, new()
        {
            using (var db = new T())
            {
                var switcher = false;
                if (switcher)
                {
                    db.CreateDatabaseIfNotExist();
                }
                else
                {
                    db.TryCreateTable();
                }
            }
        }
    }
}
