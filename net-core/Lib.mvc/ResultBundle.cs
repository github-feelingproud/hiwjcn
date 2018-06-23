using Lib.helper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Runtime.Serialization;

namespace Lib.mvc
{
    public static class ResultHelper
    {
        public static IActionResult BadRequest(string msg, object data = null) =>
            new JsonResult(new _() { success = false, msg = msg, data = data }, JsonHelper._setting);
    }

    public static class ResultExtension
    {
        public static T ThrowIfException<T, ResType>(this T res)
            where T : ResJson<ResType>
        {
            if (res.error) { throw new Exception(res.msg); }
            return res;
        }
    }

    /// <summary>
    /// 通用返回错误代码
    /// </summary>
    public enum ErrorCodeEnum : int
    {
        未登录 = -1,
        没有权限 = -2,
        跳转SSO = -3,
        账户未验证 = -4,
        账户被冻结 = -5
    }
}
