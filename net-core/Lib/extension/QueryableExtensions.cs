﻿using Lib.data;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Lib.extension
{
    /// <summary>
    /// 对Linq的扩展
    /// </summary>
    public static class QueryableExtensions
    {
        public static List<T> GetListEnsureMaxCount<T>(this IRepository<T> repo,
            Expression<Func<T, bool>> where, int count, string error_msg)
            where T : IDBTable
        {
            var list = repo.GetList(where, count);
            if (list.Count >= count)
            {
                throw new Exception(error_msg);
            }
            return list;
        }

        public static async Task<List<T>> GetListEnsureMaxCountAsync<T>(this IRepository<T> repo,
            Expression<Func<T, bool>> where, int count, string error_msg)
            where T : IDBTable
        {
            var list = await repo.GetListAsync(where, count);
            if (list.Count >= count)
            {
                throw new Exception(error_msg);
            }
            return list;
        }

        public static T FirstOrThrow_<T>(this IQueryable<T> query, string error_msg)
        {
            var model = query.FirstOrDefault();
            Com.AssertNotNull(model, error_msg);
            return model;
        }

        /// <summary>
        /// 返回非null list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static List<T> NotNullList<T>(this IQueryable<T> query)
        {
            return ConvertHelper.NotNullList(query.ToList());
        }

        /// <summary>
        /// 如果条件不为空就使用条件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public static IQueryable<T> WhereIfNotNull<T>(this IQueryable<T> query, Expression<Func<T, bool>> where)
        {
            if (where != null)
            {
                query = query.Where(where);
            }
            return query;
        }

        /// <summary>
        /// 分页
        /// </summary>
        public static IQueryable<T> QueryPage<T>(this IOrderedQueryable<T> query, int page, int pagesize)
        {
            var pager = PagerHelper.GetQueryRange(page, pagesize);
            return query.Skip(pager.skip).Take(pager.take);
        }

        /// <summary>
        /// 获取记录总数和分页总数
        /// </summary>
        public static (int item_count, int page_count) QueryRowCountAndPageCount<T>(this IQueryable<T> query, int page_size)
        {
            var item_count = query.Count();
            var page_count = PagerHelper.GetPageCount(item_count, page_size);
            return (item_count, page_count);
        }

        /// <summary>
        /// 自动分页
        /// </summary>
        public static PagerData<T> ToPagedList<T, SortColumn>(this IQueryable<T> query,
            int page, int pagesize, Expression<Func<T, SortColumn>> orderby, bool desc = true)
        {
            var data = new PagerData<T>()
            {
                Page = page,
                PageSize = pagesize
            };

            data.ItemCount = query.Count();
            data.DataList = query.OrderBy_(orderby, desc).QueryPage(page, pagesize).ToList();

            return data;
        }

        /// <summary>
        /// take别名
        /// </summary>
        public static IQueryable<T> Limit<T>(this IQueryable<T> query, int count) => query.Take(count);

        /// <summary>
        /// 排序
        /// </summary>
        public static IOrderedQueryable<T> OrderBy_<T, SortColumn>(this IQueryable<T> query,
            Expression<Func<T, SortColumn>> orderby, bool desc = true) =>
            desc ?
            query.OrderByDescending(orderby) :
            query.OrderBy(orderby);
    }
}
