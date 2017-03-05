using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Lib.extension
{
    /// <summary>
    /// BaseType是类
    /// GetInterfaces是接口
    /// IsGenericType是泛型
    /// GetGenericTypeDefinition()获取泛型类型比如Consumer<string>
    /// </summary>
    public static class TypeExtension
    {
        public static List<Type> FindTypesByBaseType<T>(this Assembly a)
        {
            return a.GetTypes().Where(x => x.BaseType != null && x.BaseType == typeof(T)).ToList();
        }
    }
}
