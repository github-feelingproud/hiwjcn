using Lib.helper;
using Lib.core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Linq;
using Common.Logging;
using System.Diagnostics;
using Lib.mvc;
using Microsoft.Extensions.Logging;
using Lib.ioc;

namespace Lib.extension
{
    /// <summary>
    /// 基本扩展
    /// </summary>
    public static class CommonLoggingExtension
    {
        public static void UseLogger(string name, Action<ILogger> func)
        {
            using (var s = IocContext.Instance.Scope())
            {
                var logger = s.Resolve_<ILoggerFactory>();
                try
                {
                    func.Invoke(logger.CreateLogger(name));
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.GetInnerExceptionAsJson());
                }
            }
        }

        public static void UseLogger<T>(Action<ILogger> func) =>
            UseLogger(typeof(T).FullName, func);

        public static void AddLog__(this Exception e, string logger) =>
            UseLogger(logger, x => x.LogError(e, e.Message));

        public static void AddInfo__(this string msg, string logger) =>
            UseLogger(logger, x => x.LogInformation(msg));

        public static void AddWarning__(this string msg, string logger) =>
            UseLogger(logger, x => x.LogWarning(msg));

        public static void AddDebug__(this string msg, string logger) =>
            UseLogger(logger, x => x.LogDebug(msg));

        //缓存各种logger
        public static readonly Dictionary<string, ILog> _str_logger_factory = new Dictionary<string, ILog>();
        public static readonly Dictionary<Type, ILog> _type_logger_factory = new Dictionary<Type, ILog>();

        public static ILog GetLogger(this string logger) =>
            LogManager.GetLogger(logger ?? throw new ArgumentNullException($"logger name is null"));

        public static ILog GetLogger(this Type logger) =>
            LogManager.GetLogger(logger ?? throw new ArgumentNullException($"logger type is null"));

        public static ILog CachedLogger(this string logger) =>
            _str_logger_factory.GetOrSet(logger, () => logger.GetLogger());

        public static ILog CachedLogger(this Type logger) =>
            _type_logger_factory.GetOrSet(logger, () => logger.GetLogger());

        public static string GetInnerExceptionAsJson(this Exception e) =>
            Com.GetExceptionMsgJson(e);

        public static List<string> GetInnerExceptionAsList(this Exception e) =>
            Com.GetExceptionMsgList(e);

        public static void DebugInfo(this Exception e) =>
            Debug.WriteLine(e.GetInnerExceptionAsJson());

        public static void DebugInfo(this string msg) =>
            Debug.WriteLine(msg);
    }

    /// <summary>
    /// 日志扩展
    /// </summary>
    public static class LogExtension
    {
        private static void Handler(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                e.DebugInfo();
            }
        }

        public static void AddLog(this Exception e, string logger) =>
            Handler(() => logger.CachedLogger().Error(e.GetInnerExceptionAsJson()));

        public static void AddLog(this Exception e, Type logger) =>
            Handler(() => logger.CachedLogger().Error(e.GetInnerExceptionAsJson()));

        public static void AddLog_(this Exception e, string logger) =>
            Handler(() => logger.CachedLogger().Error(e.GetInnerExceptionAsJson(), e));

        public static void AddLog_(this Exception e, Type logger) =>
            Handler(() => logger.CachedLogger().Error(e.GetInnerExceptionAsJson(), e));

        public static void AddErrorLog(this string log, string logger) =>
            Handler(() => logger.CachedLogger().Error(log));

        public static void AddErrorLog(this string log, Type logger) =>
            Handler(() => logger.CachedLogger().Error(log));

        public static void AddInfoLog(this string log, string logger) =>
            Handler(() => logger.CachedLogger().Info(log));

        public static void AddInfoLog(this string log, Type logger) =>
            Handler(() => logger.CachedLogger().Info(log));

        public static void AddWarnLog(this string log, string logger) =>
            Handler(() => logger.CachedLogger().Warn(log));

        public static void AddWarnLog(this string log, Type logger) =>
            Handler(() => logger.CachedLogger().Warn(log));

        public static void AddFatalLog(this string log, string logger) =>
            Handler(() => logger.CachedLogger().Fatal(log));

        public static void AddFatalLog(this string log, Type logger) =>
            Handler(() => logger.CachedLogger().Fatal(log));
    }

    /// <summary>
    /// 日志进一步封装，部分是史俊的要求
    /// </summary>
    public static class CommonLogExtension
    {
        public static readonly string LoggerName =
            ConfigurationManager.AppSettings["LoggerName"] ?? "WebLogger";

        public static readonly bool LogFullException =
            (ConfigurationManager.AppSettings["LogFullException"] ?? "true").ToBool();

        private static string FriendlyTime()
        {
            var now = DateTime.Now;
            try
            {
                var week = DateTimeHelper.GetWeek(now);

                return $"【{now.ToDateTimeString()}】-【{week}】";
            }
            catch
            {
                return $"{now}";
            }
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        public static void AddErrorLog(this Exception e, string extra_data = null)
        {
            string ExceptionJson()
            {
                try
                {
                    if (!LogFullException)
                    {
                        return "无法记录整个异常对象，请修改配置文件";
                    }
                    return JsonHelper.ObjectToJson(e);
                }
                catch (Exception err)
                {
                    return $"无法把整个{nameof(Exception)}对象转为json，原因：{err.Message}";
                }
            }

            var json = new
            {
                error_msg = e.GetInnerExceptionAsList(),
                exception_type = $"异常类型：{e.GetType()?.FullName}",
                full_exception = ExceptionJson(),
                req_data = ReqData(),
                extra_data = extra_data,
                friendly_time = FriendlyTime(),
                tips = new string[] { "建议使用json格式化工具：http://json.cn/" }
            }.ToJson();

            json.AddErrorLog(LoggerName);
        }

        /// <summary>
        /// 业务日志
        /// </summary>
        /// <param name="log"></param>
        public static void AddBusinessInfoLog(this string log)
        {
            var json = new
            {
                msg = log,
                req_data = ReqData(),
                friendly_time = FriendlyTime(),
            }.ToJson();

            json.AddInfoLog(LoggerName);
        }

        /// <summary>
        /// 业务日志
        /// </summary>
        /// <param name="log"></param>
        public static void AddBusinessWarnLog(this string log)
        {
            var json = new
            {
                msg = log,
                req_data = ReqData(),
                friendly_time = FriendlyTime(),
            }.ToJson();

            json.AddWarnLog(LoggerName);
        }

        /// <summary>
        /// 请求上下文信息 
        /// </summary>
        /// <returns></returns>
        private static object ReqData()
        {
            try
            {
                var context = HttpContext.Current;
                if (context == null)
                {
                    return new { msg = "非Web环境" };
                }

                var req_id = context.GetRequestID();
                var method = context.Request.HttpMethod;
                var header = context.Request.Headers.ToDict();
                var url = context.Request.Url.ToString();
                var p = context.Request.Form.ToDict().ToUrlParam();
                var cookies = context.Request.Cookies.AllKeys.Select(x => new
                {
                    key = x,
                    value = context.Request.Cookies[x]?.Value
                });

                //请求上下文信息
                return new
                {
                    RequestID = req_id,
                    RequestMethod = method,
                    RequestUrl = url,
                    RequestParam = p,
                    RequestHeader = header,
                    RequestCookie = cookies
                };
            }
            catch (Exception e)
            {
                return new { msg = e.Message };
            }
        }
    }
}
