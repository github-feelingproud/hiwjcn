using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;

namespace Lib.extension
{
    public static class EnumExtension
    {
        /// <summary>
        /// 用来获取枚举成员
        /// </summary>
        public static Dictionary<string, object> GetEnumFieldsValues(this Type t, bool description_first = true)
        {
            var fields = t.GetFields(BindingFlags.Static | BindingFlags.Public);

            string GetFieldName(MemberInfo m) =>
                description_first ?
                m.GetCustomAttribute<DescriptionAttribute>()?.Description ?? m.Name :
                m.Name;

            return fields.ToDictionary(x => GetFieldName(x), x => x.GetValue(null));
        }

        public static Dictionary<string, T> GetItems<T>(this Enum data)
        {
            //Enum.GetValues();
            //Enum.GetNames();

            var fields = data.GetType().GetFields(BindingFlags.Static | BindingFlags.Public);
            var keys = fields.Select(x => x.GetValue(data)).ToList();

            return fields.ToDictionary(x => x.Name, x => (T)x.GetValue(data));
        }
    }
}
