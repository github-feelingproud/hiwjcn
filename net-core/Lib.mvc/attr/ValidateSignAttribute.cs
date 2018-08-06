using Lib.core;
using Lib.extension;
using Lib.helper;
using Lib.ioc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lib.mvc.attr
{
    /// <summary>
    /// 验证签名
    /// </summary>
    public class ValidateSignAttribute : _ActionFilterBaseAttribute
    {
        /// <summary>
        /// 配置文件里的key
        /// </summary>
        public string ConfigKey { get; set; } = "sign_key";
        public string SignKey { get; set; } = "sign";

        /// <summary>
        /// 时间戳误差
        /// </summary>
        public int DeviationSeconds { get; set; } = 10;

        public override async Task OnActionExecutionAsync(ActionExecutingContext _context, ActionExecutionDelegate next)
        {
            using (var s = IocContext.Instance.Scope())
            {
                var config = s.ResolveConfig_();

                var salt = config[ConfigKey];
                if (!ValidateHelper.IsPlumpString(salt)) { throw new Exception($"没有配置签名的约定key({ConfigKey})"); }
                var context = _context.HttpContext;

                var allparams = context.PostAndGet();

                #region 验证时间戳
                var disable_timestamp_check = ConvertHelper.GetString(config["disable_timestamp_check"]).ToBool();
                if (!disable_timestamp_check)
                {
                    var timestamp = ConvertHelper.GetInt64(allparams.GetValueOrDefault("timestamp"), -1);
                    if (timestamp < 0)
                    {
                        _context.Result = ResultHelper.BadRequest("缺少时间戳");
                        return;
                    }
                    var server_timestamp = DateTimeHelper.GetTimeStamp();
                    //取绝对值
                    if (Math.Abs(server_timestamp - timestamp) > Math.Abs(DeviationSeconds))
                    {
                        _context.Result = ResultHelper.BadRequest("请求时间戳已经过期", new
                        {
                            client_timestamp = timestamp,
                            server_timestamp = server_timestamp
                        });
                        return;
                    }
                }
                #endregion

                #region 验证签名
                var disable_sign_check = ConvertHelper.GetString(config["disable_sign_check"]).ToBool();
                if (!disable_sign_check)
                {
                    var sign = ConvertHelper.GetString(allparams.GetValueOrDefault(SignKey)).ToUpper();
                    if (!ValidateHelper.IsAllPlumpString(sign))
                    {
                        _context.Result = ResultHelper.BadRequest("请求被拦截，获取不到签名");
                        return;
                    }

                    var reqparams = SignHelper.FilterAndSort(allparams, SignKey, new MyStringComparer());
                    var (md5, sign_data) = SignHelper.CreateSign(reqparams, salt);

                    if (sign != md5)
                    {
                        _context.Result = ResultHelper.BadRequest("签名错误", new
                        {
                            client_sign = md5,
                            server_sign = sign,
                            server_order = sign_data
                        });
                        return;
                    }
                }
                #endregion}

                await next.Invoke();
            }
        }
    }
}
