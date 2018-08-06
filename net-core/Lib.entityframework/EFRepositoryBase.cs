using Lib.extension;
using Lib.helper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Lib.data.ef
{
    public abstract partial class EFRepositoryBase<T> : IEFRepository<T> 
        where T : class, IDBTable
    {
        #region 添加

        public int Add(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new ArgumentException("参数为空"); }

            return PrepareSession(db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    set.Add(m);
                }
                return db.SaveChanges();
            });
        }

        public async Task<int> AddAsync(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new ArgumentException("参数为空"); }

            return await PrepareSessionAsync(async db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    set.Add(m);
                }
                return await db.SaveChangesAsync();
            });
        }
        #endregion

        #region 删除

        public int Delete(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new ArgumentException("参数为空"); }

            return PrepareSession(db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    db.Entry(m).State = EntityState.Deleted;
                    //session.Set<T>().Remove(m);
                }
                return db.SaveChanges();
            });
        }

        public async Task<int> DeleteAsync(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new ArgumentException("参数为空"); }

            return await PrepareSessionAsync(async db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    db.Entry(m).State = EntityState.Deleted;
                    //session.Set<T>().Remove(m);
                }
                return await db.SaveChangesAsync();
            });
        }

        public int DeleteWhere(Expression<Func<T, bool>> where)
        {
            return PrepareSession(db =>
            {
                var set = db.Set<T>();
                var q = set.AsQueryable();

                q = q.WhereIfNotNull(where);

                set.RemoveRange(q);

                return db.SaveChanges();
            });
        }

        public async Task<int> DeleteWhereAsync(Expression<Func<T, bool>> where)
        {
            return await PrepareSessionAsync(async db =>
            {
                var set = db.Set<T>();
                var q = set.AsQueryable();

                q = q.WhereIfNotNull(where);

                set.RemoveRange(q);

                return await db.SaveChangesAsync();
            });
        }
        #endregion

        #region 修改

        public int Update(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new ArgumentException("参数为空"); }

            return PrepareSession(db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    //添加追踪（引用），如果多个追踪对象包含相同key就会抛出异常
                    db.Entry(m).State = EntityState.Modified;
                }
                return db.SaveChanges();
            });
        }

        public async Task<int> UpdateAsync(params T[] models)
        {
            if (!ValidateHelper.IsPlumpList(models)) { throw new ArgumentException("参数为空"); }

            return await PrepareSessionAsync(async db =>
            {
                var set = db.Set<T>();
                foreach (var m in models)
                {
                    //添加追踪（引用），如果多个追踪对象包含相同key就会抛出异常
                    db.Entry(m).State = EntityState.Modified;
                }
                return await db.SaveChangesAsync();
            });
        }

        #endregion

        #region 查询

        public T GetByKeys(params object[] keys)
        {
            if (!ValidateHelper.IsPlumpList(keys)) { throw new ArgumentException("参数为空"); }

            return PrepareSession(db =>
            {
                return db.Set<T>().Find(keys);
            });
        }

        public async Task<T> GetByKeysAsync(params object[] keys)
        {
            if (!ValidateHelper.IsPlumpList(keys)) { throw new ArgumentException("参数为空"); }

            return await PrepareSessionAsync(async db =>
            {
                return await db.Set<T>().FindAsync(keys);
            });
        }

        public List<T> QueryList<OrderByColumnType>(
            Expression<Func<T, bool>> where,
            Expression<Func<T, OrderByColumnType>> orderby = null,
            bool Desc = true,
            int? start = null,
            int? count = null)
        {
            return PrepareIQueryable(query =>
            {
                query = query.WhereIfNotNull(where);

                if (orderby != null)
                {
                    query = query.OrderBy_(orderby, Desc);
                }
                if (start != null)
                {
                    if (orderby == null) { throw new ArgumentException("使用skip前必须先排序"); }
                    query = query.Skip(start.Value);
                }
                if (count != null)
                {
                    query = query.Take(count.Value);
                }
                return query.ToList();
            });
        }

        public async Task<List<T>> QueryListAsync<OrderByColumnType>(
            Expression<Func<T, bool>> where,
            Expression<Func<T, OrderByColumnType>> orderby = null,
            bool Desc = true,
            int? start = null,
            int? count = null)
        {
            return await PrepareIQueryableAsync(async query =>
            {
                query = query.WhereIfNotNull(where);

                if (orderby != null)
                {
                    query = query.OrderBy_(orderby, Desc);
                }
                if (start != null)
                {
                    if (orderby == null) { throw new ArgumentException("使用skip前必须先排序"); }
                    query = query.Skip(start.Value);
                }
                if (count != null)
                {
                    query = query.Take(count.Value);
                }
                return await query.ToListAsync();
            });
        }

        public List<T> GetList(Expression<Func<T, bool>> where, int? count = null)
        {
            return QueryList<object>(where: where, count: count);
        }
        public async Task<List<T>> GetListAsync(Expression<Func<T, bool>> where, int? count = null)
        {
            return await QueryListAsync<object>(where: where, count: count);
        }

        public T GetFirst(Expression<Func<T, bool>> where)
        {
            var list = GetList(where: where, count: 1);
            return list.FirstOrDefault();
        }
        public async Task<T> GetFirstAsync(Expression<Func<T, bool>> where)
        {
            var list = await GetListAsync(where: where, count: 1);
            return list.FirstOrDefault();
        }

        public int GetCount(Expression<Func<T, bool>> where)
        {
            return PrepareIQueryable(query =>
            {
                query = query.WhereIfNotNull(where);
                return query.Count();
            });
        }
        public async Task<int> GetCountAsync(Expression<Func<T, bool>> where)
        {
            return await PrepareIQueryableAsync(async query =>
            {
                query = query.WhereIfNotNull(where);
                return await query.CountAsync();
            });
        }

        public bool Exist(Expression<Func<T, bool>> where)
        {
            return PrepareIQueryable(query =>
            {
                query = query.WhereIfNotNull(where);
                return query.Any();
            });
        }
        public async Task<bool> ExistAsync(Expression<Func<T, bool>> where)
        {
            return await PrepareIQueryableAsync(async query =>
            {
                query = query.WhereIfNotNull(where);
                return await query.AnyAsync();
            });
        }
        #endregion

        public virtual void Dispose() { }
    }
}
