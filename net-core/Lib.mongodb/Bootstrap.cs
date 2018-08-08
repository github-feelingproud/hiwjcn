using Lib.extension;
using Lib.ioc;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lib.mongodb
{
    public static class MongoBootstrap
    {
        public static IServiceCollection UseMongoDB(this IServiceCollection collection,
            string database_name, string connection_string)
        {
            Func<IMongoClient> func = () => new MongoClient(MongoClientSettings.FromConnectionString(connection_string));

            collection.AddSingleton<IMongoClientWrapper>(_ => new MongoClientWrapper(database_name, string.Empty, func));
            collection.AddComponentDisposer<MongoDisposer>();

            return collection;
        }

        public static IServiceCollection UseMongoRepositoryFromIoc(this IServiceCollection collection) =>
            collection.UseMongoRepository(typeof(MongoRepository<>));

        public static IServiceCollection UseMongoRepository(this IServiceCollection collection, Type repoType)
        {
            if (repoType == null)
                throw new ArgumentNullException(nameof(repoType));
            if (!repoType.IsGenericType)
                throw new ArgumentException("mongo repository type must be generic type");

            collection.AddTransient(typeof(IMongoRepository<>), repoType);
            return collection;
        }
    }

    public interface IMongoClientWrapper : Lib.ioc.IServiceWrapper<IMongoClient>
    {
        string DatabaseName { get; }
    }

    public class MongoClientWrapper : Lib.ioc.LazyServiceWrapperBase<IMongoClient>, IMongoClientWrapper
    {
        private string _db_name;

        public MongoClientWrapper(string database_name, string name, Func<IMongoClient> func) : base(name, func)
        {
            this._db_name = database_name;
        }

        public string DatabaseName => this._db_name;
    }

    public class MongoDisposer : IDisposeComponent
    {
        private readonly List<IMongoClientWrapper> _clients;

        public MongoDisposer(IServiceProvider provider)
        {
            this._clients = provider.ResolveAll_<IMongoClientWrapper>().ToList();
        }

        public string ComponentName => "mongodb";

        public int DisposeOrder => 1;

        public void Dispose()
        {
            foreach (var m in this._clients)
            {
                try
                {
                    //do nothing
                }
                catch (Exception e)
                {
                    e.AddErrorLog();
                }
            }
        }
    }
}
