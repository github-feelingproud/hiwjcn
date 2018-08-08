using Lib.extension;
using Lib.helper;
using Lib.ioc;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lib.mongodb
{
    public static class Bootstrap
    {
        public static readonly string DefaultName = Com.GetUUID();

        public static IServiceCollection UseMongoDB(this IServiceCollection collection,
            string database_name, string connection_string)
        {
            Func<IMongoClient> func = () => new MongoClient(MongoClientSettings.FromConnectionString(connection_string));

            collection.AddSingleton<IMongoClientWrapper>(_ => new MongoClientWrapper(database_name, Bootstrap.DefaultName, func));
            collection.AddComponentDisposer<MongoDisposer>();

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

    public class MongoDisposer : Lib.core.IDisposeComponent
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
