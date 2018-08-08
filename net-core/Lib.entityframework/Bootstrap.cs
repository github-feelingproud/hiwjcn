using Lib.data.ef;
using Lib.helper;
using Lib.ioc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Lib.entityframework
{
    public static class EFBootstrap
    {
        public static readonly string DefaultName = Com.GetUUID();

        /// <summary>
        /// 使用EF
        /// </summary>
        public static IServiceCollection UseEF(this IServiceCollection collection, Func<DbContext> func)
        {
            collection.AddTransient<IEFContext>(_ => new EFDbContextWrapper(EFBootstrap.DefaultName, func));
            return collection;
        }

        public static IServiceCollection UseEFRepository(this IServiceCollection collection, Type repoType)
        {
            if (repoType == null)
                throw new ArgumentNullException(nameof(repoType));
            if (!repoType.IsGenericType)
                throw new ArgumentException("repository type must be generic type");

            collection.AddTransient(typeof(IEFRepository<>), repoType);
            return collection;
        }
    }

    public interface IEFContext : IServiceWrapper<DbContext> { }

    public class EFDbContextWrapper : LazyServiceWrapperBase<DbContext>, IEFContext
    {
        public EFDbContextWrapper(string name, Func<DbContext> func) :
            base(name, func)
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
