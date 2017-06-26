using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.helper;

namespace Lib.extension
{
    public static class CollectionExtension
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int batchSize)
        {
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    yield return Split(enumerator, batchSize);
                }
            }
        }

        private static IEnumerable<T> Split<T>(IEnumerator<T> enumerator, int batchSize)
        {
            do
            {
                yield return enumerator.Current;

            } while (--batchSize > 0 && enumerator.MoveNext());
        }

        /// <summary>
        /// NameValueCollection转为字典
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ToDict(this NameValueCollection col)
        {
            var dict = new Dictionary<string, string>();
            foreach (var key in col.AllKeys)
            {
                if (!ValidateHelper.IsPlumpString(key)) { continue; }
                dict[key] = col[key];
            }
            return dict;
        }

        /// <summary>
        /// 获取成员，超过索引返回默认值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <param name="deft"></param>
        /// <returns></returns>
        public static T GetItem<T>(this IList<T> list, int index, T deft = default(T))
        {
            if (index > list.Count() - 1) { return deft; }
            return list[index];
        }

        /// <summary>
        /// 生成bootstrap的表格html，未完成
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string ToBootStrapTableHtml<T>(this IEnumerable<T> list) where T : class
        {
            var html = new StringBuilder();
            var props = typeof(T).GetProperties();
            foreach (var m in list)
            {

            }
            return html.ToString();
        }

        /// <summary>
        /// 执行Reduce（逻辑和python一样）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static T Reduce<T>(this IList<T> list, Func<T, T, T> func)
        {
            if (!ValidateHelper.IsPlumpList(list))
            {
                throw new Exception($"空list无法执行{nameof(Reduce)}操作");
            }

            var res = func(list[0], list.GetItem(1));
            for (var i = 2; i < list.Count; ++i)
            {
                res = func(res, list[i]);
            }
            return res;
        }

        /// <summary>
        /// 迭代相邻两个item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="func"></param>
        public static void IterateItems<T>(this IList<T> list, Action<T, T, int, int> func)
        {
            for (var i = 0; i < list.Count - 1; ++i)
            {
                func(list[i], list[i + 1], i, i + 1);
            }
        }

        /// <summary>
        /// 判断两个list是否有相同的item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool SameItems<T>(this IList<T> list, IList<T> data)
        {
            if (list.Count != data.Count) { return false; }
            foreach (var m in list)
            {
                if (!data.Where(x => x.Equals(m)).Any())
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 判断两个list是否有相同的item
        /// </summary>
        /// <param name="list"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool SameCanOrderedItems(this IList<int> list, IList<int> data)
        {
            return ".".Join_(list.OrderBy(x => x)) == ".".Join_(data.OrderBy(x => x));
        }
    }

    /// <summary>
    /// 可以计算的list《int》
    /// </summary>
    public class ComputableList : List<int>
    {
        public ComputableList() { }

        public ComputableList(IEnumerable<int> list) : base(list) { }

        public static ComputableList operator +(ComputableList a, ComputableList b)
        {
            var list = new ComputableList();
            for (var i = 0; i < a.Count || i < b.Count; ++i)
            {
                list.Add(a.GetItem(i, 0) + b.GetItem(i, 0));
            }
            return list;
        }

        public static List<bool> operator >(ComputableList a, int b)
        {
            return a.Select(x => x > b).ToList();
        }

        public static List<bool> operator <(ComputableList a, int b)
        {
            return a.Select(x => x < b).ToList();
        }

        public static List<bool> operator >=(ComputableList a, int b)
        {
            return a.Select(x => x >= b).ToList();
        }

        public static List<bool> operator <=(ComputableList a, int b)
        {
            return a.Select(x => x <= b).ToList();
        }

        public static ComputableList operator *(ComputableList a, int b)
        {
            return new ComputableList(a.Select(x => x * b));
        }

        public static ComputableList operator /(ComputableList a, int b)
        {
            return new ComputableList(a.Select(x => x / b));
        }

        public static ComputableList operator +(ComputableList a, int b)
        {
            return new ComputableList(a.Select(x => x + b));
        }

        public static ComputableList operator -(ComputableList a, int b)
        {
            return new ComputableList(a.Select(x => x - b));
        }
    }
}
