using Lib.data.ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Lib.entityframework
{
    public static class EFBootstrap
    {
        /// <summary>
        /// 使用EF
        /// </summary>
        public static IServiceCollection UseEF<T>(this IServiceCollection collection)
            where T : DbContext
        {
            collection.AddTransient<DbContext, T>().AddTransient<T, T>();
            return collection;
        }

        /// <summary>
        /// 一个请求一个dbcontext
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IServiceCollection UseEFRepositoryFromIoc(this IServiceCollection collection) =>
            collection.UseEFRepository(typeof(EFRepository<>));

        /// <summary>
        /// 一个repo一个dbcontext
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IServiceCollection UseEFRepositoryFromIoc_(this IServiceCollection collection) =>
            collection.UseEFRepository(typeof(EFRepositoryFromIOC<>));

        /// <summary>
        /// 使用repo，type必须是泛型
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="repoType"></param>
        /// <returns></returns>
        public static IServiceCollection UseEFRepository(this IServiceCollection collection, Type repoType)
        {
            if (repoType == null)
                throw new ArgumentNullException(nameof(repoType));
            if (!repoType.IsGenericType)
                throw new ArgumentException("ef repository type must be generic type");

            collection.AddTransient(typeof(IEFRepository<>), repoType);
            return collection;
        }
    }
}
