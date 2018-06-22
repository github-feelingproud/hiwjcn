using Autofac;
using Autofac.Builder;
using Autofac.Integration.Mvc;
using Lib.cache;
using Lib.core;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Lib.extension;
using System.Data;
using System.Data.Entity;
using Lib.mq;
using Microsoft.Extensions.DependencyInjection;

namespace Lib.ioc
{
    public static class ClassFinderExtension
    {
        /// <summary>
        /// 获取可以注册的类
        /// </summary>
        public static IEnumerable<Type> FindAllRegistableClass(this Assembly a) =>
            a.GetAllNormalClass().Where(x => x.CanRegIoc());

        /// <summary>
        /// 是否注册为单例
        /// </summary>
        public static bool IsSingleInstance(this Type t) =>
            t.GetCustomAttributes_<SingleInstanceAttribute>().Any();

        /// <summary>
        /// 是否拦截实例
        /// </summary>
        public static bool IsInterceptClass(this Type t) =>
            t.GetCustomAttributes_<InterceptInstanceAttribute>().Any();

        /// <summary>
        /// 配置可以注册IOC，
        /// 要public，不是抽象类，不是接口，没有不注册属性，不是数据表model
        /// </summary>
        public static bool CanRegIoc(this Type t) =>
            t.IsPublic && !t.IsDatabaseTable() && !t.GetCustomAttributes<NotRegIocAttribute>().Any();
    }
}
