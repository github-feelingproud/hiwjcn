﻿using Lib.cache;
using Lib.ioc;
using System;
using System.Collections.Generic;

namespace Lib.data
{
    public static class AntiBadRetry
    {
        /// <summary>
        /// 防止多次尝试密码逻辑，还没开始使用
        /// </summary>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static string RedisPreventTryLogin(string key, Func<bool> func)
        {
            var time = DateTime.Now.AddMinutes(-30);
            using (var s = IocContext.Instance.Scope())
            {
                using (var cache = s.Resolve_<ICacheProvider>())
                {
                    var list = cache.Get<List<DateTime>>(key).Result;

                    if (list.Count >= 3)
                    {
                        //先判断验证码，否则提示错误
                        return "超过尝试次数";
                    }

                    if (!func.Invoke())
                    {
                        //登录失败，添加错误时间
                        list.Add(DateTime.Now);
                    }
                    cache.Set(key, list, TimeSpan.FromSeconds(Math.Abs((DateTime.Now - time).TotalSeconds)));
                    return string.Empty;
                }
            }
        }
    }
}
