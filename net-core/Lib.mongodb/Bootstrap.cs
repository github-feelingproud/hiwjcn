using Lib.helper;
using Lib.ioc;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using System;

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
        public string ComponentName => "mongodb";

        public int DisposeOrder => 1;

        public void Dispose()
        {
            //do nothing
        }
    }
}
