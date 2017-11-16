using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Lib.data.mongodb
{
    public static class MongoExtension
    {
        public static SortDefinition<T> Sort_<T, SortType>(this SortDefinitionBuilder<T> builder, Expression<Func<T, SortType>> field, bool desc)
        {
            var exp = (MemberExpression)field.Body;
            var name = exp.Member.Name;
            if (desc)
            {
                return builder.Descending(name);
            }
            else
            {
                return builder.Ascending(name);
            }
        }
    }
}
