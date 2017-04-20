using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.extension
{
    public static class CommonExtension
    {
        /// <summary>
        /// 克隆一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T CloneObject<T>(this T obj) where T : ICloneable
        {
            return (T)((ICloneable)obj).Clone();
        }

        /// <summary>
        /// 从list中随机取出一个item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ran"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T Choice<T>(this Random ran, IList<T> list)
        {
            return list[ran.Next(minValue: 0, maxValue: list.Count() - 1)];
        }
    }
}
