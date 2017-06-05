using Lib.helper;
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
            return ran.ChoiceIndexAndItem(list).item;
        }

        /// <summary>
        /// 随机抽取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ran"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static (int index, T item) ChoiceIndexAndItem<T>(this Random ran, IList<T> list)
        {
            //The maxValue for the upper-bound in the Next() method is exclusive—
            //the range includes minValue, maxValue-1, and all numbers in between.
            var index = ran.Next(minValue: 0, maxValue: list.Count());
            return (index, list[index]);
        }

        /// <summary>
        /// 随机抽取一个后从list中移除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ran"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T PopChoice<T>(this Random ran, ref List<T> list)
        {
            var data = ran.ChoiceIndexAndItem(list);
            list.RemoveAt(data.index);
            return data.item;
        }

        /// <summary>
        /// 从list中随机抽取count个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ran"></param>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<T> Sample<T>(this Random ran, IList<T> list, int count)
        {
            return new int[count].Select(x => ran.Choice(list)).ToList();
        }

        /// <summary>
        /// 随机选取索引
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ran"></param>
        /// <param name="list"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static List<int> SampleIndexs<T>(this Random ran, IList<T> list, int count)
        {
            return ran.Sample(Com.Range(list.Count).ToList(), count);
        }

        /// <summary>
        /// 打乱list的顺序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ran"></param>
        /// <param name="list"></param>
        public static void Shuffle<T>(this Random ran, ref List<T> list)
        {
            var data = new List<T>();
            while (list.Count > 0)
            {
                data.Add(ran.PopChoice(ref list));
            }
            list.AddRange(data);
        }

        /// <summary>
        /// 根据权重选择
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ran"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T ChoiceWithWeight<T>(this Random ran, IReadOnlyDictionary<T, int> source)
        {
            if (source == null || source.Count <= 0) { throw new ArgumentException(nameof(source)); }

            if (source.Count == 1) { return source.Keys.First(); }

            var total_weight = source.Sum(x => x.Value);

            var weight = ran.Next(total_weight);

            foreach (var s in source)
            {
                if (weight < s.Value)
                {
                    return s.Key;
                }
                weight -= s.Value;
            }

            throw new Exception("权重取值异常");
        }
    }
}
