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
