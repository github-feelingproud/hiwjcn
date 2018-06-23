using Lib.helper;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Lib.data.ef
{
    /// <summary>
    /// EF
    /// </summary>
    public abstract class BaseEFContext : DbContext
    {
        public BaseEFContext(DbContextOptions option) : base(option)
        {
            //
        }

        /// <summary>
        /// 注册mapping
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //注册映射
            var ass = this.FindRegisterTableFluentMappingAssembly();
            if (ValidateHelper.IsPlumpList(ass))
            {
                foreach (var a in ass)
                {
                    //手动注册
                    //modelBuilder.Configurations.Add(new UserModelMapping());
                    //通过反射自动注册
                    modelBuilder.RegisterTableFluentMapping(a);
                }
            }
        }

        protected virtual Assembly[] FindRegisterTableFluentMappingAssembly()
        {
            return new Assembly[] { };
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
        /// 实体集合
        /// new的用法搜索 override new
        /// </summary>
        public new DbSet<T> Set<T>() where T : class, IDBTable
        {
            return base.Set<T>();
        }

    }
}
