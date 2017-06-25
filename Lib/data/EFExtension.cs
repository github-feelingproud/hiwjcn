using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Lib.extension;
using System.Data.Entity.Infrastructure;

namespace Lib.data
{
    public static class EFExtension
    {
        /// <summary>
        /// 获取生成数据表的sql
        /// </summary>
        /// <returns></returns>
        public static string GetCreateTableScript<T>(this T context) where T : DbContext
        {
            var c = ((IObjectContextAdapter)context).ObjectContext;
            var sql = c.CreateDatabaseScript();
            //var exist = c.DatabaseExists();
            return sql;
        }

        /// <summary>
        /// 如果数据库不存在就创建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        public static void CreateDatabaseIfNotExist<T>(this T context) where T : DbContext
        {
            var c = ((IObjectContextAdapter)context).ObjectContext;
            if (!c.DatabaseExists())
            {
                c.CreateDatabase();
            }
        }

        /// <summary>
        /// 注册fluent mapping
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="ass"></param>
        public static void RegisterTableFluentMapping(this DbModelBuilder builder, params Assembly[] ass)
        {
            foreach (var a in ass)
            {
                var tps = a.GetTypes().Where(x =>
                x.IsClass
                && !x.IsAbstract
                && x.BaseType != null
                && x.BaseType.IsGenericType
                && x.BaseType.GetGenericTypeDefinition() == typeof(EFMappingBase<>));
                foreach (var t in tps)
                {
                    dynamic configurationInstance = Activator.CreateInstance(t);
                    //mapping
                    builder.Configurations.Add(configurationInstance);
                }
            }
        }

        /// <summary>
        /// 注册attribute mapping
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="ass"></param>
        public static void RegisterTableAttributeMapping(this DbModelBuilder builder, params Assembly[] ass)
        {
            foreach (var a in ass)
            {
                var tps = a.GetTypes().Where(x => x.IsClass && !x.IsAbstract && x.IsAssignableTo_<IDBTable>());
                foreach (var t in tps)
                {
                    builder.RegisterEntityType(t);
                }
            }
        }

        /// <summary>
        /// 获取不跟踪的IQueryable用于查询，效率更高
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="set"></param>
        /// <returns></returns>
        public static IQueryable<T> AsNoTrackingQueryable<T>(this DbSet<T> set) where T : class
        {
            return set.AsNoTracking().AsQueryable();
        }

        /// <summary>
        /// 把实体加载到EF上下文，不重复加载
        /// Nop中的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static T AttachEntityToContext<T>(this DbContext db, T entity) where T : DBTable
        {
            //little hack here until Entity Framework really supports stored procedures
            //otherwise, navigation properties of loaded entities are not loaded until an entity is attached to the context
            var set = db.Set<T>();
            var alreadyAttached = set.Local.FirstOrDefault(x => x.IID == entity.IID);
            if (alreadyAttached == null)
            {
                //attach new entity
                set.Attach(entity);
                return entity;
            }

            //entity is already loaded
            return alreadyAttached;
        }

    }
}
