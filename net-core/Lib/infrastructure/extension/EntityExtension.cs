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
        /// <summary>
        /// 初始化后返回自己
        /// </summary>
        public static T InitSelf<T>(this T model, string flag = null)
            where T : BaseEntity
        {
            model.Init(flag);
            return model;
        }

        /// <summary>
        /// 判断数据是否已经过期
        /// </summary>
        public static bool ExpireAt<T>(this T model, DateTime time, TimeSpan span)
            where T : BaseEntity
        {
            return (model.CreateTime + span) < time;
        }

        /// <summary>
        /// 更新信息后返回自己
        /// </summary>
        public static T UpdateSelf<T>(this T model)
            where T : BaseEntity
        {
            model.Update();
            return model;
        }

        public static IQueryable<T> FilterDateRange<T>(this IQueryable<T> query,
            DateTime? start, DateTime? end,
            Func<IQueryable<T>, DateTime, IQueryable<T>> start_filter,
            Func<IQueryable<T>, DateTime, IQueryable<T>> end_filter,
            bool as_date = true)
        {
            if (start != null)
            {
                if (as_date)
                {
                    start = start.Value.Date;
                }
                query = start_filter.Invoke(query, start.Value);
            }
            if (end != null)
            {
                if (as_date)
                {
                    end = end.Value.Date.AddDays(1);
                }
                query = end_filter.Invoke(query, end.Value);
            }
            return query;
        }

        public static IQueryable<T> FilterCreateDateRange<T>(this IQueryable<T> query,
            DateTime? start, DateTime? end, bool as_date = true)
            where T : BaseEntity =>
            query.FilterDateRange(
                start, end,
                (q, d) => q.Where(x => x.CreateTime >= d),
                (q, d) => q.Where(x => x.CreateTime < d),
                as_date);

        public static IQueryable<T> FilterUpdateDateRange<T>(this IQueryable<T> query,
            DateTime? start, DateTime? end, bool as_date = true)
            where T : BaseEntity =>
            query.FilterDateRange(
                start, end,
                (q, d) => q.Where(x => x.UpdateTime >= d),
                (q, d) => q.Where(x => x.UpdateTime < d),
                as_date);

        /// <summary>
        /// check input=>delete by uids
        /// </summary>
        public static async Task<_<int>> DeleteByIds<T>(this IRepository<T> repo, params string[] uids)
            where T : BaseEntity
        {
            var data = new _<int>();
            if (!ValidateHelper.IsPlumpList(uids))
            {
                data.SetErrorMsg("ID为空");
                return data;
            }
            var count = await repo.DeleteWhereAsync(x => uids.Contains(x.UID));
            data.SetSuccessData(count);
            return data;
        }

        /// <summary>
        /// init=>check=>save
        /// </summary>
        public static async Task<_<T>> AddEntity_<T>(this IRepository<T> repo,
            T model, string model_flag)
            where T : BaseEntity
        {
            var data = new _<T>();

            model.Init(model_flag);
            if (!model.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await repo.AddAsync(model) > 0)
            {
                data.SetSuccessData(model);
                return data;
            }

            throw new Exception("保存失败");
        }
    }
}
