using Lib.helper;
using System;
using System.Web.SessionState;

namespace Lib.mvc
{
    public static class SessionHelper
    {
        public static T PopSession<T>(HttpSessionState session, string key)
        {
            var value = GetSession<T>(session, key);
            RemoveSession(session, key);
            return value;
        }

        public static T GetSession<T>(HttpSessionState session, string key)
        {
            if (session == null) { throw new Exception("session不存在"); }
            var obj = session[key];
            if (ValidateHelper.Is<T>(obj))
            {
                return (T)obj;
            }
            return default(T);
        }

        public static void SetSession(HttpSessionState session, string key, object value)
        {
            if (session == null) { throw new Exception("session不存在"); }
            session[key] = value;
        }

        public static void RemoveSession(HttpSessionState session, params string[] keys)
        {
            if (session == null) { throw new Exception("session不存在"); }
            if (!ValidateHelper.IsPlumpList(keys)) { throw new Exception("缺少keys"); }
            foreach (var key in keys)
            {
                session.Remove(key);
            }
        }
    }
}
