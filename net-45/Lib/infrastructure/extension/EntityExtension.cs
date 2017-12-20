using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using Lib.infrastructure.entity;
using Lib.mvc;
using Lib.helper;
using Lib.extension;
using System.Data.Entity;

namespace Lib.infrastructure.extension
{
    public static class EntityExtension
    {
        public static IQueryable<T> FilterDateRange<T>(this IQueryable<T> query,
            DateTime? start, DateTime? end,
            Func<IQueryable<T>, DateTime, IQueryable<T>> start_filter,
            Func<IQueryable<T>, DateTime, IQueryable<T>> end_filter,
            bool drop_time = true)
            where T : BaseEntity
        {
            if (start != null)
            {
                if (drop_time)
                {
                    start = start.Value.Date;
                }
                query = start_filter.Invoke(query, start.Value);
            }
            if (end != null)
            {
                if (drop_time)
                {
                    end = end.Value.Date.AddDays(1);
                }
                query = end_filter.Invoke(query, end.Value);
            }
            return query;
        }

        public static IQueryable<T> FilterCreateDateRange<T>(this IQueryable<T> query,
            DateTime? start, DateTime? end, bool drop_time = true)
            where T : BaseEntity =>
            query.FilterDateRange(
                start, end,
                (q, d) => q.Where(x => x.CreateTime >= d),
                (q, d) => q.Where(x => x.CreateTime < d),
                drop_time);

        public static IQueryable<T> FilterUpdateDateRange<T>(this IQueryable<T> query,
            DateTime? start, DateTime? end, bool drop_time = true)
            where T : BaseEntity =>
            query.FilterDateRange(
                start, end,
                (q, d) => q.Where(x => x.UpdateTime >= d),
                (q, d) => q.Where(x => x.UpdateTime < d),
                drop_time);

        public static async Task<_<string>> DeleteByMultipleUIDS_<T>(this IRepository<T> repo, params string[] uids)
            where T : BaseEntity
        {
            var data = new _<string>();
            if (!ValidateHelper.IsPlumpList(uids))
            {
                data.SetErrorMsg("ID为空");
                return data;
            }
            if (await repo.DeleteWhereAsync(x => uids.Contains(x.UID)) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("删除数据错误");
        }

        public static async Task<_<string>> AddEntity_<T>(this IRepository<T> repo, T model, string model_flag)
            where T : BaseEntity
        {
            var data = new _<string>();

            model.Init(model_flag);
            if (!model.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await repo.AddAsync(model) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("保存失败");
        }
    }
}
