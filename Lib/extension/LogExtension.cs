using Lib.helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Linq;
using Lib.mvc;

namespace Lib.extension
{
    public static class LogExtension
    {
        #region exception扩展
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
        /// 获取深层的异常信息
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static List<string> GetInnerExceptionAsList(this Exception e)
        {
            return Com.GetExceptionMsgList(e);
        }
        #endregion

        #region 日志扩展
        /// <summary>
        /// 使用log4net添加日志
        /// </summary>
        /// <param name="e"></param>
        /// <param name="name"></param>
        public static void AddLog(this Exception e, string name)
        {
            e.GetInnerExceptionAsJson().AddErrorLog(name);
        }
        /// <summary>
        /// 使用log4net添加日志
        /// </summary>
        /// <param name="e"></param>
        /// <param name="t"></param>
        public static void AddLog(this Exception e, Type t)
        {
            e.GetInnerExceptionAsJson().AddErrorLog(t);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="name"></param>
        public static void AddErrorLog(this string log, string name)
        {
            LogHelper.Error(name, log);
        }
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="t"></param>
        public static void AddErrorLog(this string log, Type t)
        {
            LogHelper.Error(t, log);
        }

        /// <summary>
        /// 保存信息日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="name"></param>
        public static void AddInfoLog(this string log, string name)
        {
            LogHelper.Info(name, log);
        }
        /// <summary>
        /// 保存信息日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="t"></param>
        public static void AddInfoLog(this string log, Type t)
        {
            LogHelper.Info(t, log);
        }

        /// <summary>
        /// 保存警告日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="name"></param>
        public static void AddWarnLog(this string log, string name)
        {
            LogHelper.Warn(name, log);
        }
        /// <summary>
        /// 保存警告日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="t"></param>
        public static void AddWarnLog(this string log, Type t)
        {
            LogHelper.Warn(t, log);
        }
        #endregion
    }

    public static class CommonLogExtension
    {
        public static readonly string LoggerName = ConfigurationManager.AppSettings["LoggerName"] ?? "WebLogger";

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="e"></param>
        /// <param name="prefix"></param>
        public static void AddErrorLog(this Exception e, string extra_data = null)
        {
            var json = new
            {
                error_msg = e.GetInnerExceptionAsList(),
                req_data = ReqData(),
                extra_data = extra_data
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
                req_data = ReqData()
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
                req_data = ReqData()
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

                var req_id = Com.GetRequestID();

                var method = context.Request.HttpMethod;

                var url = context.Request.Url.ToString();

                var p = context.Request.Form.ToDict().ToUrlParam();

                var cookies = context.Request.Cookies.AllKeys.Select(x =>
                new
                {
                    key = x,
                    value = context.Request.Cookies[x]?.Value
                });

                //请求上下文信息
                return new
                {
                    method = method,
                    url = url,
                    req_param = p,
                    cookies = cookies
                };
            }
            catch
            {
                return new { msg = "非Web环境" };
            }
        }
    }
}
