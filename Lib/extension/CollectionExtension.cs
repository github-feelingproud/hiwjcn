using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.extension
{
    public static class CollectionExtension
    {
        #region Split
        /// <summary>将一维集合分割成二维集合</summary>
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
        #endregion

        #region SplitWhile
        /// <summary>将一维集合分割成二维集合</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate">true：当前集合的最后一个元素</param>
        public static IEnumerable<IEnumerable<T>> SplitWhile<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    yield return SplitWhile(enumerator, predicate);
                }
            }
        }

        /// <summary>将一维集合分割成二维集合</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate">true：当前集合的最后一个元素</param>
        public static IEnumerable<IEnumerable<T>> SplitWhile<T>(this IEnumerable<T> source, Func<T, int, int, bool> predicate)
        {
            using (var enumerator = source.GetEnumerator())
            {
                var loopNumber = 0;
                while (enumerator.MoveNext())
                {
                    yield return SplitWhile(enumerator, loopNumber++, predicate);
                }
            }
        }

        private static IEnumerable<T> SplitWhile<T>(IEnumerator<T> enumerator, Func<T, bool> predicate)
        {
            do
            {
                var current = enumerator.Current;
                yield return current;

                if (predicate(current))
                    break;

            } while (enumerator.MoveNext());
        }

        private static IEnumerable<T> SplitWhile<T>(IEnumerator<T> enumerator, int loopNumber, Func<T, int, int, bool> predicate)
        {
            var index = 0;
            do
            {
                var current = enumerator.Current;

                yield return current;

                if (predicate(current, loopNumber, index++))
                    break;

            } while (enumerator.MoveNext());
        }
        #endregion
    }
}
