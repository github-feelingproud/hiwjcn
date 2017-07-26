using Lib.extension;
using Lib.helper;
using Lib.infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Lib.data
{
    /// <summary>
    /// 标准sql中使用groupby需要有聚合函数（mysql除外），所以没有封装。
    /// 如果使用groupby查询请手写session或者iqueryable查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class EFRepository<T> : IRepository<T> where T : class, IDBTable
    {
        public EFManager _EFManager { get; private set; }

        public EFRepository() : this("db") { }
        public EFRepository(string db_name)
        {
            this._EFManager = EFManager.SelectDB(db_name);
        }

        //private const int DEFAULT_START = -1;
        //private const int DEFAULT_COUNT = 1000;

        #region 添加
        /// <summary>
        /// 添加多个model
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public int Add(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new Exception("参数为空"); }
            int count = 0;
            PrepareSession(db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    set.Add(m);
                }
                count = db.SaveChanges();
                return true;
            });
            return count;
        }
        /// <summary>
        /// 异步添加
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task<int> AddAsync(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new Exception("参数为空"); }
            int count = 0;
            await PrepareSessionAsync(async db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    set.Add(m);
                }
                count = await db.SaveChangesAsync();
                return true;
            });
            return count;
        }
        #endregion

        #region 删除
        /// <summary>
        /// 删除多个model
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public int Delete(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new Exception("参数为空"); }
            int count = 0;
            PrepareSession(db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    db.Entry(m).State = EntityState.Deleted;
                    //session.Set<T>().Remove(m);
                }
                count = db.SaveChanges();
                return true;
            });
            return count;
        }
        /// <summary>
        /// 异步删除 
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task<int> DeleteAsync(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new Exception("参数为空"); }
            var count = 0;
            await PrepareSessionAsync(async db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    db.Entry(m).State = EntityState.Deleted;
                    //session.Set<T>().Remove(m);
                }
                count = await db.SaveChangesAsync();
                return true;
            });
            return count;
        }



        public int DeleteWhere(Expression<Func<T, bool>> where)
        {
            var count = 0;
            PrepareSession(db =>
            {
                var set = db.Set<T>();
                var q = set.AsQueryable();
                if (where != null)
                {
                    q = q.Where(where);
                }

                set.RemoveRange(q);
                count = db.SaveChanges();
                return true;
            });
            return count;
        }

        public async Task<int> DeleteWhereAsync(Expression<Func<T, bool>> where)
        {
            var count = 0;
            await PrepareSessionAsync(async db =>
            {
                var set = db.Set<T>();
                var q = set.AsQueryable();
                if (where != null)
                {
                    q = q.Where(where);
                }

                set.RemoveRange(q);
                count = await db.SaveChangesAsync();
                return true;
            });
            return count;
        }
        #endregion

        #region 修改
        /// <summary>
        /// 更新多个model
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public int Update(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new Exception("参数为空"); }
            int count = 0;
            PrepareSession(db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    //添加追踪（引用），如果多个追踪对象包含相同key就会抛出异常
                    db.Entry(m).State = EntityState.Modified;
                }
                count = db.SaveChanges();
                return true;
            });
            return count;
        }
        /// <summary>
        /// 异步更新
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public async Task<int> UpdateAsync(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new Exception("参数为空"); }
            var count = 0;
            await PrepareSessionAsync(async db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    //添加追踪（引用），如果多个追踪对象包含相同key就会抛出异常
                    db.Entry(m).State = EntityState.Modified;
                }
                count = await db.SaveChangesAsync();
                return true;
            });
            return count;
        }

        #endregion

        #region 查询
        /// <summary>
        /// 根据主键查询实体
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public T GetByKeys(params object[] keys)
        {
            if (!ValidateHelper.IsPlumpList(keys)) { throw new Exception("参数为空"); }
            T model = default(T);
            PrepareSession(db =>
            {
                model = db.Set<T>().Find(keys);
                return true;
            });
            return model;
        }

        /// <summary>
        /// 异步查找
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public async Task<T> GetByKeysAsync(params object[] keys)
        {
            if (!ValidateHelper.IsPlumpList(keys)) { throw new Exception("参数为空"); }
            var model = default(T);

            await PrepareSessionAsync(async db =>
            {
                model = await db.Set<T>().FindAsync(keys);
                return true;
            });

            return model;
        }

        /// <summary>
        /// 获取list
        /// expression和func的使用注意点，参见lib的readme
        /// </summary>
        /// <param name="where">where条件</param>
        /// <param name="orderby">排序条件</param>
        /// <param name="start">开始位置</param>
        /// <param name="count">读取条数</param>
        /// <param name="Desc">正序反序</param>
        /// <returns></returns>
        public List<T> QueryList<OrderByColumnType>(
            Expression<Func<T, bool>> where,
            Expression<Func<T, OrderByColumnType>> orderby = null,
            bool Desc = true,
            int? start = null,
            int? count = null)
        {
            List<T> list = null;
            PrepareIQueryable(query =>
            {
                if (where != null)
                {
                    query = query.Where(where);
                }
                if (orderby != null)
                {
                    if (Desc)
                    {
                        query = query.OrderByDescending(orderby);
                    }
                    else
                    {
                        query = query.OrderBy(orderby);
                    }
                }
                if (start != null)
                {
                    if (orderby == null) { throw new Exception("使用skip前必须先排序"); }
                    query = query.Skip(start.Value);
                }
                if (count != null)
                {
                    query = query.Take(count.Value);
                }
                list = query.ToList();
                return true;
            });
            return ConvertHelper.NotNullList(list);
        }

        public async Task<List<T>> QueryListAsync<OrderByColumnType>(
            Expression<Func<T, bool>> where,
            Expression<Func<T, OrderByColumnType>> orderby = null,
            bool Desc = true,
            int? start = null,
            int? count = null)
        {
            List<T> list = null;
            await PrepareIQueryableAsync(async query =>
            {
                if (where != null)
                {
                    query = query.Where(where);
                }
                if (orderby != null)
                {
                    if (Desc)
                    {
                        query = query.OrderByDescending(orderby);
                    }
                    else
                    {
                        query = query.OrderBy(orderby);
                    }
                }
                if (start != null)
                {
                    if (orderby == null) { throw new Exception("使用skip前必须先排序"); }
                    query = query.Skip(start.Value);
                }
                if (count != null)
                {
                    query = query.Take(count.Value);
                }
                list = await query.ToListAsync();
                return true;
            });
            return ConvertHelper.NotNullList(list);
        }

        /// <summary>
        /// 获取list
        /// </summary>
        /// <param name="where"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<T> GetList(Expression<Func<T, bool>> where, int? count = null)
        {
            return QueryList<object>(where: where, count: count);
        }
        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> where, int? count = null)
        {
            return await QueryListAsync<object>(where: where, count: count);
        }

        /// <summary>
        /// 查询第一个
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public T GetFirst(Expression<Func<T, bool>> where)
        {
            var list = GetList(where: where, count: 1);
            if (ValidateHelper.IsPlumpList(list))
            {
                return list[0];
            }
            return default(T);
        }
        public async Task<T> GetFirstAsync(Expression<Func<T, bool>> where)
        {
            var list = await GetListAsync(where: where, count: 1);
            if (ValidateHelper.IsPlumpList(list))
            {
                return list[0];
            }
            return default(T);
        }

        /// <summary>
        /// 查询记录数（判断记录是否存在请使用Exist方法，效率更高）
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public int GetCount(Expression<Func<T, bool>> where)
        {
            int count = 0;
            PrepareIQueryable(query =>
            {
                if (where != null)
                {
                    query = query.Where(where);
                }
                count = query.Count();
                return true;
            });
            return count;
        }
        public async Task<int> GetCountAsync(Expression<Func<T, bool>> where)
        {
            int count = 0;
            await PrepareIQueryableAsync(async query =>
            {
                if (where != null)
                {
                    query = query.Where(where);
                }
                count = await query.CountAsync();
                return true;
            });
            return count;
        }

        /// <summary>
        /// 查询是否存在 
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public bool Exist(Expression<Func<T, bool>> where)
        {
            bool? ret = null;
            PrepareIQueryable(query =>
            {
                if (where != null)
                {
                    query = query.Where(where);
                }
                ret = query.Any();
                return true;
            });
            if (ret == null) { throw new Exception("Exist查询失败"); }
            return ret.Value;
        }
        public async Task<bool> ExistAsync(Expression<Func<T, bool>> where)
        {
            bool? ret = null;
            await PrepareIQueryableAsync(async query =>
            {
                if (where != null)
                {
                    query = query.Where(where);
                }
                ret = await query.AnyAsync();
                return true;
            });
            if (ret == null) { throw new Exception("Exist查询失败"); }
            return ret.Value;
        }
        #endregion

        #region 获取查询上下文
        /// <summary>
        /// 获取IQueryable对象，用于linq查询
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="track"></param>
        public void PrepareIQueryable(Func<IQueryable<T>, bool> callback, bool track = false)
        {
            this._EFManager.PrepareIQueryable<T>(callback, track);
        }
        public async Task PrepareIQueryableAsync(Func<IQueryable<T>, Task<bool>> callback, bool track = false)
        {
            await this._EFManager.PrepareIQueryableAsync<T>(callback, track);
        }

        public void PrepareSession(Func<DbContext, bool> callback)
        {
            this._EFManager.PrepareSession(callback);
        }
        public async Task PrepareSessionAsync(Func<DbContext, Task<bool>> callback)
        {
            await this._EFManager.PrepareSessionAsync(callback);
        }
        #endregion
    }

    /// <summary>
    /// TODO 准备把异步方法放到这个里面
    /// </summary>
    partial class EFRepository<T>
    {
        public void TODO_PLACE_ASYNC_METHOD_TO_THIS_PARTIAL_CLASS() { }
    }
}
