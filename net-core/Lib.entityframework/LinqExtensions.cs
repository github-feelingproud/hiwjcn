using Lib.extension;
using Lib.helper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Lib.entityframework
{
    /// <summary>
    /// 对Linq的扩展
    /// </summary>
    public static class LinqExtensions
    {
        public static async Task<T> FirstOrThrowAsync<T>(this IQueryable<T> query, string error_msg)
        {
            var model = await query.FirstOrDefaultAsync();
            Com.AssertNotNull(model, error_msg);
            return model;
        }

        /// <summary>
        /// 返回非null list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static async Task<List<T>> NotNullListAsync<T>(this IQueryable<T> query)
        {
            return ConvertHelper.NotNullList(await query.ToListAsync());
        }

        /// <summary>
        /// 获取记录总数和分页总数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="page_size"></param>
        /// <returns></returns>
        public static async Task<(int item_count, int page_count)> QueryRowCountAndPageCountAsync<T>(this IQueryable<T> query, int page_size)
        {
            var item_count = await query.CountAsync();
            var page_count = PagerHelper.GetPageCount(item_count, page_size);
            return (item_count, page_count);
        }

        /// <summary>
        /// 自动分页
        /// </summary>
        public static async Task<PagerData<T>> ToPagedListAsync<T, SortColumn>(this IQueryable<T> query,
            int page, int pagesize, Expression<Func<T, SortColumn>> orderby, bool desc = true)
        {
            var data = new PagerData<T>()
            {
                Page = page,
                PageSize = pagesize
            };

            data.ItemCount = await query.CountAsync();
            data.DataList = await query.OrderBy_(orderby, desc).QueryPage(page, pagesize).ToListAsync();

            return data;
        }
    }
}
