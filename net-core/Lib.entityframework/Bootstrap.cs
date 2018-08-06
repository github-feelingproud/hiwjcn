using Lib.ioc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Lib.entityframework
{
    public static class Bootstrap
    {
        /// <summary>
        /// 使用EF
        /// </summary>
        public static IServiceCollection UseEF<T>(this IServiceCollection collection)
            where T : DbContext
            => collection.AddTransient<DbContext, T>();

        public static IServiceCollection UseEF<T>(this IServiceCollection collection, string name, Func<T> func)
            where T : DbContext
        {
            collection.AddTransient<IServiceWrapper<T>>(_ => new DbContextWrapper<T>(name, func));
            return collection;
        }
    }

    public class DbContextWrapper<T> : LazyServiceWrapperBase<T> 
        where T : DbContext
    {
        public DbContextWrapper(string name, Func<T> func) : base(name, func)
        {
            //
        }
    }
}
