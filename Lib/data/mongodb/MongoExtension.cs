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
            if (field.Body is MemberExpression exp)
            {
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
            else
            {
                throw new Exception("不支持的排序lambda表达式");
            }
        }
    }
}
