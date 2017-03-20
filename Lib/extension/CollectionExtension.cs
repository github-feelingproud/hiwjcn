using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
