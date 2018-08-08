using Lib.data;
using Lib.helper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Lib.mongodb
{
    public interface IMongoRepository<T> : ILinqRepository<T>, IRepository<T>
        where T : MongoEntityBase
    {
        void PrepareMongoCollection(Action<IMongoCollection<T>> callback);

        Task PrepareMongoCollectionAsync(Func<IMongoCollection<T>, Task> callback);

        R PrepareMongoCollection<R>(Func<IMongoCollection<T>, R> callback);

        Task<R> PrepareMongoCollectionAsync<R>(Func<IMongoCollection<T>, Task<R>> callback);

        List<T> QueryNearBy(Expression<Func<T, bool>> where, int page, int pagesize,
            Expression<Func<T, object>> field, GeoInfo point,
            double? max_distance = null, double? min_distance = null);
    }
}
