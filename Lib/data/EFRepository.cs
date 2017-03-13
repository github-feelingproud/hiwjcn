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
    public class EFRepository<T> : IRepository<T> where T : DBTable
    {
        public EFManager _EFManager { get; private set; }

        public EFRepository() : this("db") { }
        public EFRepository(string db_name)
        {
            this._EFManager = EFManager.SelectDB(db_name);
        }

        private const int DEFAULT_START = -1;
        private const int DEFAULT_COUNT = 1000;

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
            _EFManager.PrepareSession(db =>
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
            await _EFManager.PrepareSessionAsync(async db =>
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
            _EFManager.PrepareSession(db =>
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
            using (var db = this._EFManager.GetDbContext())
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    db.Entry(m).State = EntityState.Deleted;
                    //session.Set<T>().Remove(m);
                }
                return await db.SaveChangesAsync();
            }
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
            _EFManager.PrepareSession(db =>
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
            using (var db = this._EFManager.GetDbContext())
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    //添加追踪（引用），如果多个追踪对象包含相同key就会抛出异常
                    db.Entry(m).State = EntityState.Modified;
                }
                return await db.SaveChangesAsync();
            }
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
            _EFManager.PrepareSession((db) =>
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
            using (var db = _EFManager.GetDbContext())
            {
                return await db.Set<T>().FindAsync(keys);
            }
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
            int start = DEFAULT_START,
            int count = DEFAULT_COUNT)
        {
            List<T> list = null;
            PrepareIQueryable((query) =>
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
                if (start >= 0)
                {
                    if (orderby == null) { throw new Exception("使用skip前必须先用orderby排序"); }
                    query = query.Skip(start);
                }
                if (count > 0)
                {
                    query = query.Take(count);
                }
                list = query.NotNullList();
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
        public List<T> GetList(Expression<Func<T, bool>> where, int count = DEFAULT_COUNT)
        {
            return QueryList<object>(where: where, count: count);
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
            if (ret == null) { throw new Exception("是否存在查询失败"); }
            return ret.Value;
        }
        #endregion

        /// <summary>
        /// 获取IQueryable对象，用于linq查询
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="Transaction"></param>
        public void PrepareIQueryable(Func<IQueryable<T>, bool> callback, bool track = true)
        {
            this._EFManager.PrepareIQueryable<T>(callback, track);
        }

        public void PrepareSession(Func<DbContext, bool> callback)
        {
            this._EFManager.PrepareSession(callback);
        }
    }
}
