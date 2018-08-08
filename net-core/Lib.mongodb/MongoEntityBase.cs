using Lib.data;
using MongoDB.Bson;
using System;

namespace Lib.mongodb
{
    [Serializable]
    public abstract class MongoEntityBase : IDBTable
    {
        public virtual ObjectId _id { get; set; }
    }
}
