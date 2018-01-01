using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Lib.extension
{
    public static class EnumExtension
    {
        /// <summary>
        /// 用来获取枚举成员
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetPublicStaticFieldsValues(this Type t)
        {
            var fields = t.GetFields(BindingFlags.Static | BindingFlags.Public);
            return fields.ToDictionary(x => x.Name, x => x.GetValue(null));
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
