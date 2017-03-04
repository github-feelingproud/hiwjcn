using Lib.helper;
using System;

namespace Lib.extension
{
    public static class LogExtension
    {
        /// <summary>
        /// 获取深层的异常信息
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string GetInnerExceptionAsJson(this Exception e)
        {
            return Com.GetExceptionMsgJson(e);
        }

        /// <summary>
        /// 使用log4net添加日志
        /// </summary>
        /// <param name="e"></param>
        /// <param name="name"></param>
        /// <param name="prefix"></param>
        public static void AddLog(this Exception e, string name = nameof(AddLog))
        {
            e.GetInnerExceptionAsJson().SaveErrorLog(name);
        }
        /// <summary>
        /// 使用log4net添加日志
        /// </summary>
        /// <param name="e"></param>
        /// <param name="t"></param>
        /// <param name="prefix"></param>
        public static void AddLog(this Exception e, Type t)
        {
            e.GetInnerExceptionAsJson().SaveErrorLog(t);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="name"></param>
        public static void SaveErrorLog(this string log, string name = nameof(SaveErrorLog))
        {
            LogHelper.Error(name, log);
        }
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="t"></param>
        public static void SaveErrorLog(this string log, Type t)
        {
            LogHelper.Error(t, log);
        }

        /// <summary>
        /// 保存信息日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="name"></param>
        public static void SaveInfoLog(this string log, string name = nameof(SaveInfoLog))
        {
            LogHelper.Info(name, log);
        }
        /// <summary>
        /// 保存信息日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="t"></param>
        public static void SaveInfoLog(this string log, Type t)
        {
            LogHelper.Info(t, log);
        }

        /// <summary>
        /// 保存警告日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="name"></param>
        public static void SaveWarnLog(this string log, string name = nameof(SaveWarnLog))
        {
            LogHelper.Warn(name, log);
        }
        /// <summary>
        /// 保存警告日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="t"></param>
        public static void SaveWarnLog(this string log, Type t)
        {
            LogHelper.Info(t, log);
        }
    }
}
