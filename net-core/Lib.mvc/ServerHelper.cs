using Lib.cache;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Lib.mvc
{
    public static class ServerHelper
    {
        /// <summary>
        /// 在请求上下文中缓存对象,不能缓存null对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static T CacheInHttpContext<T>(this HttpContext context, string key, Func<T> func)
        {
            if (context.Items.ContainsKey(key))
            {
                var obj = context.Items[key];
                if (obj != null && obj is CacheResult<T> data)
                {
                    return data.Result;
                }
                else
                {
                    return default(T);
                }
            }
            var d = func.Invoke();
            context.Items[key] = new CacheResult<T>() { Result = d, Success = true };
            return d;
        }

        public static async Task<T> CacheInHttpContextAsync<T>(this HttpContext context, string key, Func<Task<T>> func)
        {
            if (context.Items.ContainsKey(key))
            {
                var obj = context.Items[key];
                if (obj != null && obj is CacheResult<T> data)
                {
                    return data.Result;
                }
                else
                {
                    return default(T);
                }
            }
            var d = await func.Invoke();
            if (d != null)
            {
                context.Items[key] = new CacheResult<T>() { Result = d, Success = true };
            }
            return d;
        }
    }
}
