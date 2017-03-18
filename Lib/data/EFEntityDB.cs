using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Lib.data
{
    public class EFEntityDB : DbContext
    {
        public EFEntityDB() : base("")
        {
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ValidateOnSaveEnabled = false;

            this.Database.CommandTimeout = 3;
            this.Database.Log = (log) =>
            {
                //if (!ValidateHelper.IsPlumpString(log)) { return; }
                //LogHelper.Error(typeof(EntityDB), "EF日志：\n" + log);
            };
        }

        /// <summary>
        /// 注册mapping
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //不在表名后加s或者es
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            bool UseThis = false;
            if (!UseThis) { return; }

            //手动注册
            //modelBuilder.Configurations.Add(new UserModelMapping());
            //通过反射自动注册
            modelBuilder.RegisterTableFluentMapping(this.GetType().Assembly);

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Attach an entity to the context or return an already attached entity (if it was already attached)
        /// 附加一个实体，如果已经存在就直接返回
        /// </summary>
        /// <typeparam name="TEntity">TEntity</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Attached entity</returns>
        //protected virtual TEntity AttachEntityToContext<TEntity>(TEntity entity) where TEntity : ModelBase, new()
        //{
        //    //little hack here until Entity Framework really supports stored procedures
        //    //otherwise, navigation properties of loaded entities are not loaded until an entity is attached to the context
        //    var alreadyAttached = Set<TEntity>().Local.FirstOrDefault(x => x.Id == entity.Id);
        //    if (alreadyAttached == null)
        //    {
        //        //attach new entity
        //        Set<TEntity>().Attach(entity);
        //        return entity;
        //    }

        //    //entity is already loaded
        //    return alreadyAttached;
        //}

        /// <summary>
        /// 获取生成数据表的sql
        /// </summary>
        /// <returns></returns>
        public string CreateTables()
        {
            var sql = ((IObjectContextAdapter)this).ObjectContext.CreateDatabaseScript();
            return sql;
        }

        /// <summary>
        /// 实体集合
        /// new的用法搜索 override new
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public new IDbSet<T> Set<T>() where T : class, IDBTable
        {
            return base.Set<T>();
        }

    }
}
