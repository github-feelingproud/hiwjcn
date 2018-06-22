using MongoDB.Bson;
using System;

namespace Lib.data.mongodb
{
    [Serializable]
    public abstract class MongoEntityBase : IDBTable
    {
        public virtual ObjectId _id { get; set; }
    }
}
