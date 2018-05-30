using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Lib.helper;
using System.Collections;

namespace Lib.extension
{
    public static class CollectionExtension
    {
        /// <summary>
        /// 加入非空集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="data"></param>
        public static void AddWhenNotEmpty<T>(this List<T> list, IEnumerable<T> data)
        {
            if (ValidateHelper.IsPlumpList(data))
            {
                list.AddRange(data);
            }
        }

        public static List<T> AddRange_<T>(this List<T> list, params T[] items)
        {
            if (ValidateHelper.IsPlumpList(items))
            {
                list.AddRange(items);
            }
            return list;
        }

        /// <summary>
        /// 取第一个非空
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string FirstNotEmpty_(this IEnumerable<string> list) =>
            list.Where(x => ValidateHelper.IsPlumpString(x)).FirstOrDefault();

        /// <summary>
        /// 排除空字符串并去重
        /// </summary>
        public static IEnumerable<string> NotEmptyAndDistinct<T>(this IEnumerable<T> list, Func<T, string> target) =>
                list.Select(x => target.Invoke(x)).Where(x => ValidateHelper.IsPlumpString(x)).Distinct();

        /// <summary>
        /// 更新集合，多删少补
        /// </summary>
        public static (IEnumerable<T> WaitForDelete, IEnumerable<T> WaitForAdd) UpdateList<T>(this IEnumerable<T> old_list,
            IEnumerable<T> new_list, IEqualityComparer<T> comparer = null)
        {
            new_list = new_list ?? throw new ArgumentNullException(nameof(new_list));

            var delete_list = old_list.Except_(new_list, comparer).ToList();
            var create_list = new_list.Except_(old_list, comparer).ToList();

            return (delete_list, create_list);
        }

        /// <summary>
        /// 更新集合，多删少补
        /// </summary>
        public static (IEnumerable<Target> WaitForDelete, IEnumerable<Target> WaitForAdd) UpdateList<T, Target>(this IEnumerable<T> old_list,
            IEnumerable<T> new_list, Func<T, Target> selector, IEqualityComparer<Target> comparer = null) =>
            old_list.Select(selector).UpdateList(new_list.Select(selector), comparer);

        /// <summary>
        /// 更新集合，多删少补
        /// </summary>
        public static (IEnumerable<T> WaitForDelete, IEnumerable<T> WaitForAdd) UpdateList_<T, Target>(this IEnumerable<T> old_list,
            IEnumerable<T> new_list, Func<T, Target> selector, IEqualityComparer<Target> comparer = null)
        {
            var change = old_list.UpdateList(new_list, selector, comparer);

            var delete_list = old_list.Where(x => change.WaitForDelete.Contains(selector.Invoke(x))).ToList();
            var create_list = new_list.Where(x => change.WaitForAdd.Contains(selector.Invoke(x))).ToList();

            return (delete_list, create_list);
        }

        /// <summary>
        /// 转为可迭代实体
        /// </summary>
        public static IEnumerable<T> AsEnumerable_<T>(this IEnumerable collection)
        {
            foreach (T item in collection)
            {
                yield return item;
            }
        }

        /// <summary>
        /// 移除
        /// </summary>
        public static void RemoveWhere_<T>(this List<T> list, Func<T, bool> where)
        {
            for (var i = list.Count - 1; i >= 0; --i)
            {
                var item = list[i];
                if (where.Invoke(item))
                {
                    list.Remove(item);
                }
            }
        }

        /// <summary>
        /// 包含
        /// </summary>
        public static bool ContainsItem_<T>(this IEnumerable<T> list, T data, IEqualityComparer<T> comparer = null)
        {
            if (list == null) { throw new ArgumentNullException(nameof(list)); }

            return comparer == null ?
                list.Contains(data) :
                list.Contains(data, comparer);
        }

        /// <summary>
        /// 全部包含
        /// </summary>
        public static bool ContainsMany_<T>(this IEnumerable<T> list, IEnumerable<T> data,
            IEqualityComparer<T> comparer = null) =>
            (data ?? throw new ArgumentNullException(nameof(data))).All(x => list.ContainsItem_(x, comparer));

        /// <summary>
        /// 是否在集合中
        /// </summary>
        [Obsolete("不推荐使用")]
        public static bool In<T>(this T data, IEnumerable<T> list, IEqualityComparer<T> comparer = null)
        {
            if (list == null) { throw new ArgumentNullException(nameof(list)); }

            return list.ContainsItem_(data, comparer);
        }

        /// <summary>
        /// 获取两个集合的交集
        /// </summary>
        public static List<T> GetInterSection<T>(this IEnumerable<T> list, IEnumerable<T> data,
            IEqualityComparer<T> comparer = null) =>
            Com.GetInterSection(list, data, comparer);

        /// <summary>
        /// 有交集
        /// </summary>
        public static bool AnyEqual<T>(this IEnumerable<T> list, IEnumerable<T> data,
            IEqualityComparer<T> comparer = null) =>
            list.GetInterSection(data, comparer).Count() > 0;

        /// <summary>
        /// 有相同的item
        /// </summary>
        public static bool AllEqual<T>(this IEnumerable<T> list, IEnumerable<T> data,
            IEqualityComparer<T> comparer = null) =>
            list.Count() == data.Count() && list.ContainsMany_(data, comparer);

        /// <summary>
        /// 除了（排除）
        /// </summary>
        public static IEnumerable<T> Except_<T>(this IEnumerable<T> list, IEnumerable<T> data,
            IEqualityComparer<T> comparer = null)
        {
            //list.Except(data, comparer);

            data = data ?? new List<T>() { };

            return list.Where(x => !data.ContainsItem_(x, comparer));
        }

        /// <summary>
        /// 在list中添加item，遇到重复就抛异常
        /// </summary>
        public static void AddOnceOrThrow(this List<string> list, string flag, string error_msg = null)
        {
            if (list.Contains(flag))
            {
                throw new Exception(error_msg ?? $"{flag}已经存在");
            }
            list.Add(flag);
        }

        /// <summary>
        /// 空集合就抛异常
        /// </summary>
        public static IEnumerable<T> ThrowIfEmpty<T>(this IEnumerable<T> list, string msg = null)
        {
            if (!list.Any()) { throw new Exception(msg ?? "不允许的空集合"); }
            return list;
        }

        /// <summary>
        /// 集合数量超出最大值就抛出异常
        /// </summary>
        public static IEnumerable<T> EnsureMaxCount<T>(this IEnumerable<T> list, int max, string msg = null)
        {
            var count = list.Count();
            if (count > max)
            {
                throw new Exception(msg ?? $"集合数量({count})超过了允许最大值：{max}");
            }
            return list;
        }

        /// <summary>
        /// 拼接item.tostring()
        /// </summary>
        public static string AsString<T>(this IEnumerable<T> list) =>
            list.Select(x => x?.ToString()).ConcatString();

        /// <summary>
        /// 链接字符串
        /// </summary>
        public static string ConcatString(this IEnumerable<string> list)
        {
            var data = new StringBuilder();
            foreach (var item in list.Where(x => x != null))
            {
                data.Append(item);
            }
            return data.ToString();
        }

        /// <summary>
        /// 变成a=>b=>c=>d
        /// </summary>
        public static string AsSteps(this IEnumerable<string> list, string sp = "=>")
        {
            return (sp ?? throw new Exception(nameof(sp))).Join_(list);
        }

        /// <summary>
        /// 如果不是空就添加到日志
        /// </summary>
        public static void AddBusinessInfoLogIfNotEmpty(this IEnumerable<string> list, string prefix = null)
        {
            if (ValidateHelper.IsPlumpList(list))
            {
                $"{prefix ?? string.Empty}{list.ToJson()}".AddBusinessInfoLog();
            }
        }

        /// <summary>
        /// 如果不是空就添加到日志
        /// </summary>
        public static void AddErrorLogIfNotEmpty(this IEnumerable<string> list, string prefix = null)
        {
            if (ValidateHelper.IsPlumpList(list))
            {
                new Exception($"{prefix ?? string.Empty}{list.ToJson()}").AddErrorLog();
            }
        }

        /// <summary>
        /// 解决ilist没有foreach的问题
        /// </summary>
        public static void ForEach_<T>(this IEnumerable<T> list, Action<int, T> action)
        {
            var index = 0;
            foreach (var m in list)
            {
                action.Invoke(index++, m);
            }
        }

        /// <summary>
        /// 解决ilist没有foreach的问题
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="action"></param>
        public static void ForEach_<T>(this IEnumerable<T> list, Action<T> action)
        {
            list.ForEach_((index, x) => action.Invoke(x));
        }

        /// <summary>
        /// 解决ilist没有foreach的问题
        /// </summary>
        public static async Task ForEachAsync_<T>(this IEnumerable<T> list, Func<int, T, Task> action)
        {
            var index = 0;
            foreach (var m in list)
            {
                await action.Invoke(index++, m);
            }
        }

        /// <summary>
        /// 解决ilist没有foreach的问题
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="action"></param>
        public static async Task ForEachAsync_<T>(this IEnumerable<T> list, Func<T, Task> action)
        {
            await list.ForEachAsync_(async (index, x) => await action.Invoke(x));
        }

        /// <summary>
        /// 反转
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> Reverse_<T>(this IEnumerable<T> list)
        {
            var data = list.ToList();
            Com.ReversalList(ref data);
            return data;
        }

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
        public static T GetItem<T>(this IList<T> list, int index, T deft = default(T))
        {
            if (index >= 0 && index <= list.Count - 1) { return list[index]; }
            return deft;
        }

        /// <summary>
        /// 执行Reduce（逻辑和python一样），至少2个item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static T Reduce<T>(this IList<T> list, Func<T, T, T> func)
        {
            if (!ValidateHelper.IsPlumpList(list) || list.Count < 2)
            {
                throw new Exception($"item少于2的list无法执行{nameof(Reduce)}操作");
            }

            var res = func(list[0], list[1]);
            for (var i = 2; i < list.Count; ++i)
            {
                res = func(res, list[i]);
            }
            return res;
        }

        /// <summary>
        /// 迭代相邻两个item
        /// </summary>
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
        /// 取出一批数据
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Batch<T>([NotNull]this IEnumerable<T> list, int size)
        {
            if (size <= 0) { throw new Exception("batch size必须大于0"); }
            var temp = new List<T>();
            foreach (var m in list)
            {
                temp.Add(m);
                if (temp.Count >= size)
                {
                    yield return temp;
                    //清空，开始下一个batch
                    temp = new List<T>();
                }
            }
            if (temp.Count > 0)
            {
                yield return temp;
            }
        }

        /// <summary>
        /// 判断两个list是否有相同的item
        /// </summary>
        /// <param name="list"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool SameCanOrderedItems<T>(this IList<T> list, IList<T> data)
        {
            var tps = new Type[]
            {
                typeof(int),
                typeof(long),
                typeof(double),
                typeof(string),
                typeof(byte),
                typeof(float),
                typeof(decimal),
                typeof(DateTime),

                typeof(int?),
                typeof(long?),
                typeof(double?),
                typeof(byte?),
                typeof(float?),
                typeof(decimal?),
                typeof(DateTime?)
            };
            if (!tps.Contains(typeof(T))) { throw new Exception("不支持的数据类型对比"); }
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
