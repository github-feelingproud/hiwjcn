using Lib.helper;
using System;
using System.Web.SessionState;
using Lib.extension;

namespace Lib.mvc
{
    public static class SessionHelper
    {
        /// <summary>
        /// 取出后删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T PopSession<T>(HttpSessionState session, string key)
        {
            var value = GetSession<T>(session, key);
            RemoveSession(session, key);
            return value;
        }

        /// <summary>
        /// 获取session
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetSession<T>(HttpSessionState session, string key)
        {
            var obj = session[key];

            {
                //用新的方法实现
                if (obj is T res)
                {
                    return res;
                }
            }

            if (ValidateHelper.Is<T>(obj))
            {
                return (T)obj;
            }

            if (obj == null) { return default(T); }

            try
            {
                return obj.ToJson().JsonToEntity<T>();
            }
            catch
            {
                return default(T);
            }
        }

        /// <summary>
        /// 保存到session
        /// </summary>
        /// <param name="session"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetSession(HttpSessionState session, string key, object value)
        {
            session[key] = value;
        }

        /// <summary>
        /// 删除session
        /// </summary>
        /// <param name="session"></param>
        /// <param name="keys"></param>
        public static void RemoveSession(HttpSessionState session, params string[] keys)
        {
            foreach (var key in keys)
            {
                session.Remove(key);
            }
        }
    }
}
