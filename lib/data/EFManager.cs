using Lib.core;
using Lib.helper;
using Lib.ioc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;

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
        /// 加速首次启动 
        /// </summary>
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
        public DbContext GetDbContext()
        {
            if (!AppContext.IsRegistered<DbContext>(db_name))
            {
                throw new Exception("请在容器中注册dbcontext实例");
            }
            return AppContext.GetObject<DbContext>(name: db_name);
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
                    query = session.Set<T>().AsNoTracking();
                }
                return callback.Invoke(query);
            });
        }

        /// <summary>
        /// 使用数据连接
        /// </summary>
        /// <param name="callback"></param>
        public void PrepareConnection(Action<IDbConnection> callback)
        {
            using (var db = AppContext.GetObject<IDbConnection>())
            {
                db.ConnectionString = ConfigHelper.Instance.MySqlConnectionString;
                db.Open();
                callback.Invoke(db);
            }
        }

        /// <summary>
        /// 使用原始的数据连接（未测试）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        public void PrepareConnection(Func<IDbConnection, IDbTransaction, bool> callback, IsolationLevel? isoLevel = null)
        {
            PrepareConnection(db =>
            {
                IDbTransaction t = null;
                try
                {
                    if (isoLevel == null)
                    {
                        t = db.BeginTransaction();
                    }
                    else
                    {
                        t = db.BeginTransaction(isoLevel.Value);
                    }

                    if (callback.Invoke(db, t))
                    {
                        t.Commit();
                    }
                    else
                    {
                        t.Rollback();
                    }
                }
                catch (Exception e)
                {
                    t?.Rollback();
                    throw e;
                }
                finally
                {
                    t?.Dispose();
                }
            });
        }

    }
}
