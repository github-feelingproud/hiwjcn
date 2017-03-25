using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Lib.data
{
    public class DapperLinq
    {
    }
    public static class DapperLinqExtension
    {
        public static string ToSql(this Expression exp)
        {
            var sqlbuilder = new StringBuilder();
            if (exp is BinaryExpression)
            {
                var node = (BinaryExpression)exp;
                return node.Left.ToSql() + node.NodeType + node.Right.ToSql();
            }
            if (exp is ParameterExpression)
            {
                var node = (ParameterExpression)exp;
            }
            if (exp is MemberExpression)
            {
                var node = (MemberExpression)exp;
            }
            return sqlbuilder.ToString();
        }
    }
}
