using Lib.cache;
using Lib.core;
using Lib.helper;
using System;
using System.Linq;
using Lib.data;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lib.ioc;
using Lib.extension;
using System.Threading.Tasks;

namespace Lib.infrastructure
{
    /// <summary>
    /// 逻辑类基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ServiceBase<T> : IServiceBase<T> where T : class, IDBTable
    {
        /// <summary>
        /// 是否使用缓存
        /// </summary>
        public bool UseCache { get; set; }

        /// <summary>
        /// 缓存时间，分钟
        /// </summary>
        public int CacheExpiresMinutes { get; set; }

        [Obsolete("不要直接使用，使用_DalBase")]
        private IRepository<T> _db { get; set; }

        /// <summary>
        /// 数据访问
        /// </summary>
        protected IRepository<T> _DalBase
        {
            get
            {
                if (this._db == null)
                {
                    this._db = AppContext.GetObject<IRepository<T>>();
                }
                return this._db;
            }
        }

        /// <summary>
        /// 逻辑类基类
        /// </summary>
        public ServiceBase()
        {
            UseCache = false;
            CacheExpiresMinutes = ConfigHelper.Instance.CacheExpiresMinutes;
        }

        /// <summary>
        /// 记录上一个状态是否使用缓存
        /// </summary>
        private bool? _UseCacheStatus { get; set; }

        /// <summary>
        /// 临时设置是否使用缓存
        /// </summary>
        /// <param name="usecache"></param>
        public void SetUseCacheValueTemporary(bool usecache)
        {
            _UseCacheStatus = UseCache;
            UseCache = usecache;
        }

        /// <summary>
        /// 还原到临时设置使用缓存的前一个状态
        /// </summary>
        public void RestoreUseCacheValue()
        {
            if (_UseCacheStatus == null) { throw new Exception("必须先调用临时设置是否缓存的方法"); }
            UseCache = _UseCacheStatus.Value;
            _UseCacheStatus = null;
        }

        /// <summary>
        /// 缓存
        /// </summary>
        protected CacheType Cache<CacheType>(string key, Func<CacheType> dataSource)
        {
            return CacheManager.Cache(key, dataSource,
                UseCache: UseCache, expires_minutes: CacheExpiresMinutes);
        }

        /// <summary>
        /// 缓存
        /// </summary>
        protected async Task<CacheType> CacheAsync<CacheType>(string key, Func<Task<CacheType>> dataSource)
        {
            return await CacheManager.CacheAsync(key, dataSource,
                UseCache: UseCache, expires_minutes: CacheExpiresMinutes);
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key"></param>
        protected void RemoveCache(string key)
        {
            CacheManager.RemoveCache(key);
        }

        /// <summary>
        /// 通过正则删除缓存，效率不高，谨慎使用
        /// </summary>
        /// <param name="pattern"></param>
        protected void RemoveCacheByPattern(string pattern)
        {
            CacheManager.RemoveByPattern(pattern);
        }

        /// <summary>
        /// 获取缓存的key
        /// </summary>
        /// <param name="preStr"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        protected string GetCacheKey(string preStr, params string[] ps)
        {
            return Com.GetCacheKey(preStr, ps);
        }

        /// <summary>
        /// 执行成功标记
        /// </summary>
        public string SUCCESS { get { return string.Empty; } }

        /// <summary>
        /// 检查model约束（约等于数据库约束）
        /// 默认使用attribute验证
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual string CheckModel(T model)
        {
            var errors = CheckEntity(model);
            if (ValidateHelper.IsPlumpList(errors))
            {
                return errors[0];
            }
            return SUCCESS;
        }

        /// <summary>
        /// 调用可以override的那个checkmodel检查
        /// </summary>
        /// <param name="model"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool CheckModel(T model, out string msg)
        {
            msg = CheckModel(model);
            return !ValidateHelper.IsPlumpString(msg);
        }

        /// <summary>
        /// 找到实体
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public T FindFirstEntity(Expression<Func<T, bool>> where)
        {
            return _DalBase.GetFirst(where);
        }

        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AddEntity(T model)
        {
            var err = CheckModel(model);
            if (ValidateHelper.IsPlumpString(err)) { return err; }
            return _DalBase.Add(model) > 0 ? SUCCESS : "添加失败";
        }

        /// <summary>
        /// 直接删除对象
        /// </summary>
        /// <param name="where"></param>
        /// <returns></returns>
        public string DeleteSingleEntity(Expression<Func<T, bool>> where)
        {
            return DeleteSingleEntity(where, _ => true);
        }

        /// <summary>
        /// 判断可以删除后删除
        /// </summary>
        /// <param name="where"></param>
        /// <param name="CanDelete"></param>
        /// <returns></returns>
        public string DeleteSingleEntity(Expression<Func<T, bool>> where, Func<T, bool> CanDelete)
        {
            var list = _DalBase.GetList(where, 2);
            if (!ValidateHelper.IsPlumpList(list)) { return "数据不存在"; }
            if (list.Count() > 1) { return "当前条件下有多条记录"; }
            var model = list[0];
            if (CanDelete.Invoke(model))
            {
                //del
                return _DalBase.Delete(model) > 0 ? SUCCESS : "删除失败";
            }
            else
            {
                return "不能删除";
            }
        }

        /// <summary>
        /// 更新对象，返回非空字符串终止更新
        /// </summary>
        /// <param name="where"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public string UpdateSingleEntity(Expression<Func<T, bool>> where, RefFunc<T, string> handler)
        {
            var list = _DalBase.GetList(where, 2);
            if (!ValidateHelper.IsPlumpList(list)) { return "数据不存在"; }
            if (list.Count > 1) { return "当前条件下有多条记录"; }
            var model = list[0];
            var err = handler.Invoke(ref model);
            if (ValidateHelper.IsPlumpString(err)) { return err; }
            err = CheckModel(model);
            if (ValidateHelper.IsPlumpString(err)) { return err; }
            return _DalBase.Update(model) > 0 ? SUCCESS : "更新失败";
        }

        /// <summary>
        /// 通过标签检查实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public List<string> CheckEntity(T model)
        {
            return ValidateHelper.CheckEntity_(model);
        }
    }
}
