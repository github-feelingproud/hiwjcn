using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;
using Lib.helper;

namespace Lib.infrastructure.extension
{
    public static class CalendarExtension
    {
        public static void QueryEvents<T>(this IEnumerable<T> query, DateTime start, DateTime end)
            where T : CalendarEventTimeEntity
        {
            var dayofweeks = Com.Range(start, end, TimeSpan.FromDays(1))
                .Select(x => (int)x.DayOfWeek).Distinct().ToList();

            query = query.Where(x =>
            x.Start < end &&
            (end == null || end > start));
        }
    }
}
