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
}
