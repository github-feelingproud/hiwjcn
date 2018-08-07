using Lib.ioc;
using Lib.helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using Lib.data.ef;

namespace Lib.entityframework
{
    public static class Bootstrap
    {
        public static readonly string DefaultName = Com.GetUUID();

        /// <summary>
        /// 使用EF
        /// </summary>
        public static IServiceCollection UseEF<T>(this IServiceCollection collection, Func<T> func)
            where T : DbContext
        {
            collection.AddTransient<IServiceWrapper<T>>(_ => new DbContextWrapper<T>(Bootstrap.DefaultName, func));
            return collection;
        }

        public static IServiceCollection UseEFRepository(this IServiceCollection collection, Type repoType)
        {
            collection.AddTransient(typeof(IEFRepository<>), repoType);
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

        public override void Dispose()
        {
            base.Dispose();
            if (this._lazy.IsValueCreated)
                this._lazy.Value.Dispose();
        }
    }
}
