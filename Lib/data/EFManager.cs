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

namespace Lib.data
{
    /// <summary>
    /// EF的帮助类
    /// </summary>
    public class EFManager
    {
        public EFManager(string name)
        {
            this.db_name = name;
        }
        private string db_name { get; set; } = "db";
        public static EFManager SelectDB(string name = null)
        {
            return new EFManager(name);
        }

        #region 静态方法
        /// <summary>
        /// 取消EF首次访问数据库的System.Data.Entity.CreateDatabaseIfNotExists策略
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        public static void SetNoInitializer<T>(T db) where T : DbContext
        {
            SetNoInitializer<T>();
        }
        public static void SetNoInitializer<T>() where T : DbContext
        {
            var type = typeof(T);
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
        public static void TryInstallDatabase()
        {
            using (var db = EFManager.SelectDB("db").GetDbContext())
            {
                var switcher = true;
                if (switcher)
                {
                    db.CreateDatabaseIfNotExist();
                }
                else
                {
                    var sql = db.GetCreateTableScript();

                    using (db.Database.Connection)
                    {
                        db.Database.Connection.OpenIfClosedWithRetry();
                        using (var t = db.Database.Connection.StartTransaction())
                        {
                            try
                            {
                                db.Database.Connection.Execute(sql, transaction: t);
                                t.Commit();
                            }
                            catch (Exception e)
                            {
                                t.Rollback();
                                e.AddErrorLog("创建数据表失败，可能已经存在");
                            }
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 加速首次启动 
        /// </summary>
        [Obsolete("不要用")]
        public void FastStart()
        {
            //Database.SetInitializer<EntityDB>(null);
            PrepareSession(db =>
            {
                SetNoInitializer(db);

                var objectContext = ((IObjectContextAdapter)db).ObjectContext;
                var mappingCollection = (StorageMappingItemCollection)objectContext.MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);
                mappingCollection.GenerateViews(new List<EdmSchemaError>());
                return true;
            });
        }

        /// <summary>
        /// 获取错误
        /// </summary>
        /// <returns></returns>
        public string GetValidationErrors()
        {
            using (var db = GetDbContext())
            {
                return JsonHelper.ObjectToJson(db.GetValidationErrors());
            }
        }

        /// <summary>
        /// 获取实体对象
        /// </summary>
        /// <returns></returns>
        public DbContext GetDbContext(string db = null)
        {
            /* 提升性能，不要检查
            if (!AppContext.IsRegistered<DbContext>(db_name))
            {
                throw new Exception("请在容器中注册dbcontext实例");
            }*/
            return AppContext.GetObject<DbContext>(name: db ?? db_name);
        }

        /// <summary>
        /// 准备实体容器对象
        /// </summary>
        /// <param name="callback"></param>
        public void PrepareSession(Func<DbContext, bool> callback)
        {
            using (var db = GetDbContext())
            {
                //执行回调
                if (!callback.Invoke(db))
                {
                    //执行失败
                }
            }
        }

        public async Task PrepareSessionAsync(Func<DbContext, Task<bool>> callback)
        {
            using (var db = GetDbContext())
            {
                //执行回调
                if (!(await callback.Invoke(db)))
                {
                    //执行失败
                }
            }
        }

        /// <summary>
        /// 准备IQueryable用于linq查询
        /// Where方法里不是func而是expression，
        /// 这个不会用来执行只会用来分析表达式树，不会出现空指针现象（x.age==18）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        /// <param name="Transaction"></param>
        public void PrepareIQueryable<T>(Func<IQueryable<T>, bool> callback, bool track = true)
            where T : class, IDBTable
        {
            PrepareSession(session =>
            {
                //用来查询，所以不要跟踪实体状态
                //NopCommerce:
                //Gets a table with "no tracking" enabled (EF feature) 
                //Use it only when you load record(s) only for read-only operations
                IQueryable<T> query = null;
                if (track)
                {
                    query = session.Set<T>().AsQueryable();
                }
                else
                {
                    query = session.Set<T>().AsNoTrackingQueryable();
                }
                return callback.Invoke(query);
            });
        }

        public async Task PrepareIQueryableAsync<T>(Func<IQueryable<T>, Task<bool>> callback, bool track = true)
            where T : class, IDBTable
        {
            await PrepareSessionAsync(async session =>
            {
                //用来查询，所以不要跟踪实体状态
                //NopCommerce:
                //Gets a table with "no tracking" enabled (EF feature) 
                //Use it only when you load record(s) only for read-only operations
                IQueryable<T> query = null;
                if (track)
                {
                    query = session.Set<T>().AsQueryable();
                }
                else
                {
                    query = session.Set<T>().AsNoTracking();
                }
                return await callback.Invoke(query);
            });
        }

    }
}
