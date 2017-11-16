using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB;
using System.Linq.Expressions;

namespace Lib.data.mongodb
{
    public interface IMongoRepository<T> : ILinqRepository<T>, IRepository<T>
        where T : MongoEntityBase
    {
        void PrepareMongoCollection(Action<IMongoCollection<T>> callback);

        Task PrepareMongoCollectionAsync(Func<IMongoCollection<T>, Task> callback);

        R PrepareMongoCollection<R>(Action<IMongoCollection<T>, R> callback);

        Task<R> PrepareMongoCollectionAsync<R>(Func<IMongoCollection<T>, Task<R>> callback);

    }
}
