using Lib.extension;
using MongoDB.Driver;
using System;

namespace Lib.data.mongodb
{
    public abstract class MongoContextBase : IDisposable
    {
        protected readonly IMongoClient _client;
        protected readonly IMongoDatabase _database;

        public MongoContextBase(string connectionString, string database)
        {
            this._client = new MongoClient(connectionString);
            this._database = this._client.GetDatabase(database);
        }

        public virtual IMongoCollection<T> Set<T>() where T : MongoEntityBase
        {
            return this._database.GetCollection<T>(typeof(T).GetTableName());
        }

        public void Dispose()
        {
            // do nothing
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

}
