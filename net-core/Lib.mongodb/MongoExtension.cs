using Lib.helper;
using MongoDB.Driver;
using System;
using System.Linq.Expressions;

namespace Lib.mongodb
{
    public static class MongoExtension
    {
        public static SortDefinition<T> Sort_<T, SortType>(this SortDefinitionBuilder<T> builder, 
            Expression<Func<T, SortType>> field, bool desc)
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
                throw new NotSupportedException("不支持的排序lambda表达式");
            }
        }

        public static IFindFluent<T, T> Take<T>(this IFindFluent<T, T> finder, int take) =>
            finder.Limit(take);

        public static IFindFluent<T, T> QueryPage<T>(this IFindFluent<T, T> finder, int page, int pagesize)
        {
            var range = PagerHelper.GetQueryRange(page, pagesize);
            return finder.Skip(range.skip).Take(range.take);
        }
    }
}
