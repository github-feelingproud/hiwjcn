using Lib.core;
using Lib.helper;
using Lib.ioc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lib.extension
{
    /// <summary>
    /// 基本扩展
    /// </summary>
    public static class CommonLoggingHelper
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

        public static void UseLogger(Type tp, Action<ILogger> func) =>
            CommonLoggingHelper.UseLogger(tp.FullName, func);

        public static void UseLogger<T>(Action<ILogger> func) =>
            CommonLoggingHelper.UseLogger(typeof(T).FullName, func);
    }

    /// <summary>
    /// 日志扩展
    /// </summary>
    public static class LogExtension
    {
        public static void AddLog(this Exception e, string logger, params object[] args) =>
            CommonLoggingHelper.UseLogger(logger, x => x.LogError(e, e.Message, args));

        public static void AddLog(this Exception e, Type logger, params object[] args) =>
            CommonLoggingHelper.UseLogger(logger, x => x.LogError(e, e.Message, args));

        public static void AddErrorLog(this string log, string logger, params object[] args) =>
            CommonLoggingHelper.UseLogger(logger, x => x.LogError(log, args));

        public static void AddErrorLog(this string log, Type logger, params object[] args) =>
            CommonLoggingHelper.UseLogger(logger, x => x.LogError(log, args));

        public static void AddInfoLog(this string log, string logger, params object[] args) =>
            CommonLoggingHelper.UseLogger(logger, x => x.LogInformation(log, args));

        public static void AddInfoLog(this string log, Type logger, params object[] args) =>
            CommonLoggingHelper.UseLogger(logger, x => x.LogInformation(log, args));

        public static void AddWarnLog(this string log, string logger, params object[] args) =>
            CommonLoggingHelper.UseLogger(logger, x => x.LogWarning(log, args));

        public static void AddWarnLog(this string log, Type logger, params object[] args) =>
            CommonLoggingHelper.UseLogger(logger, x => x.LogWarning(log, args));

        public static void AddFatalLog(this string log, string logger, params object[] args) =>
            CommonLoggingHelper.UseLogger(logger, x => x.LogCritical(log, args));

        public static void AddFatalLog(this string log, Type logger, params object[] args) =>
            CommonLoggingHelper.UseLogger(logger, x => x.LogCritical(log, args));

        public static string GetInnerExceptionAsJson(this Exception e) =>
            Com.GetExceptionMsgJson(e);

        public static List<string> GetInnerExceptionAsList(this Exception e) =>
            Com.GetExceptionMsgList(e);
    }

    /// <summary>
    /// 附加信息
    /// </summary>
    public interface IAttachExtraLogInformation
    {
        object AttachOrThrow();
    }

    /// <summary>
    /// 日志进一步封装，部分是史俊的要求
    /// </summary>
    public static class CommonLogExtension
    {
        private static string LoggerName(IServiceScope s) =>
            s.ResolveConfig_()["LoggerName"] ?? "WebLogger";

        private static bool LogFullException(IServiceScope s) =>
            (s.ResolveConfig_()["LogFullException"] ?? "false").ToBool();


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
                    return JsonHelper.ObjectToJson(e);
                }
                catch (Exception err)
                {
                    return $"无法把整个{nameof(Exception)}对象转为json，原因：{err.Message}";
                }
            }


            using (var s = IocContext.Instance.Scope())
            {
                var data = new
                {
                    error_msg = e.GetInnerExceptionAsList(),
                    exception_type = $"异常类型：{e.GetType()?.FullName}",
                    full_exception = LogFullException(s) ? ExceptionJson() : string.Empty,
                    req_data = ReqData(),
                    extra_data = extra_data,
                    friendly_time = FriendlyTime(),
                    tips = new string[] { "建议使用json格式化工具：http://json.cn/" }
                };
                e.AddLog(LoggerName(s), data);
            }
        }

        /// <summary>
        /// 业务日志
        /// </summary>
        /// <param name="log"></param>
        public static void AddBusinessInfoLog(this string log)
        {
            var data = new
            {
                msg = log,
                req_data = ReqData(),
                friendly_time = FriendlyTime(),
            };

            using (var s = IocContext.Instance.Scope())
                log.AddInfoLog(LoggerName(s), data);
        }

        /// <summary>
        /// 业务日志
        /// </summary>
        /// <param name="log"></param>
        public static void AddBusinessWarnLog(this string log)
        {
            var data = new
            {
                msg = log,
                req_data = ReqData(),
                friendly_time = FriendlyTime(),
            };

            using (var s = IocContext.Instance.Scope())
                log.AddWarnLog(LoggerName(s), data);
        }

        /// <summary>
        /// 请求上下文信息 
        /// </summary>
        /// <returns></returns>
        private static object ReqData()
        {
            try
            {
                using (var s = IocContext.Instance.Scope())
                {
                    var service = s.ResolveOptional_<IAttachExtraLogInformation>() ?? throw new MsgException("无附加信息");

                    var data = service.AttachOrThrow();
                    return data;
                }
                /*
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
                };*/
            }
            catch (Exception e)
            {
                return new { msg = e.Message };
            }
        }
    }
}
