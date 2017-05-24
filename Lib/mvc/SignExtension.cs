using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Lib.extension;
using Lib.core;
using Lib.helper;
using System.Configuration;

namespace Lib.mvc
{
    public static class SignExtension
    {
        public static readonly string sign_key = ConfigurationManager.AppSettings[nameof(sign_key)] ?? "sign";
        public static readonly string sign_salt = ConfigurationManager.AppSettings[nameof(sign_salt)] ?? "";
        public static readonly string timestamp_key = ConfigurationManager.AppSettings[nameof(timestamp_key)] ?? "timestamp";

        /// <summary>
        /// 获取用来做签名的数据
        /// </summary>
        public static Dictionary<string, string> PostAndGetForSign(this HttpContext context)
        {
            var dict = context.PostAndGet();
            Func<KeyValuePair<string, string>, bool> filter = x =>
            {
                if (x.Key == null || x.Key == sign_key || x.Key.Length > 32 || x.Value?.Length > 32)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            };
            return dict.Where(x => filter(x)).ToDictionary(x => x.Key, x => ConvertHelper.GetString(x.Value));
        }

        /// <summary>
        /// 计算签名
        /// </summary>
        public static (string sign, string sign_data) GetSign(this HttpContext context)
        {
            var reqparams = context.PostAndGetForSign();

            return SignHelper.CreateSign(reqparams, sign_salt);
        }

        /// <summary>
        /// 验证时间戳
        /// </summary>
        public static (bool success, string msg, long client_timestamp, long server_timestamp) ValidTimeStamp(
            this HttpContext context, int DeviationSeconds = 10)
        {
            var reqparams = context.PostAndGet();
            var server_timestamp = DateTimeHelper.GetTimeStamp();
            var timestamp = ConvertHelper.GetInt64(reqparams["timestamp"], -1);
            if (timestamp < 0)
            {
                return (false, "缺少时间戳", timestamp, server_timestamp);
            }
            //取绝对值
            if (Math.Abs(server_timestamp - timestamp) > Math.Abs(DeviationSeconds))
            {
                return (false, "请求时间戳已经过期", timestamp, server_timestamp);
            }
            return (true, string.Empty, timestamp, server_timestamp);
        }
    }

    public static class SignHelper
    {
        public static (string sign, string sign_data) CreateSign(Dictionary<string, string> reqparams, string salt)
        {
            var dict = new SortedDictionary<string, string>(reqparams, new MyStringComparer());
            var strdata = dict.ToUrlParam();
            if (ValidateHelper.IsPlumpString(salt))
            {
                strdata += salt;
            }
            strdata = strdata.ToLower();

            var md5 = strdata.ToMD5().ToUpper();
            return (md5, strdata);
        }
    }
}
