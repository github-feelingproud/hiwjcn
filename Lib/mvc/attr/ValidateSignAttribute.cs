using Lib.core;
using Lib.extension;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Lib.mvc.attr
{
    /// <summary>
    /// 验证签名
    /// </summary>
    public class ValidateSignAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 配置文件里的key
        /// </summary>
        public string ConfigKey { get; set; } = "sign_key";

        /// <summary>
        /// 时间戳误差
        /// </summary>
        public int DeviationSeconds { get; set; } = 10;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var sign_key = ConfigurationManager.AppSettings[ConfigKey];
            if (!ValidateHelper.IsPlumpString(sign_key)) { throw new Exception($"没有配置签名的约定key({ConfigKey})"); }

            var reqparams = new NameValueCollection();
            NameValueCollectionHelper.AddToNameValueCollection(ref reqparams, filterContext.HttpContext.Request.Form);
            NameValueCollectionHelper.AddToNameValueCollection(ref reqparams, filterContext.HttpContext.Request.QueryString);

            var disable_timestamp_check = ConvertHelper.GetString(ConfigurationManager.AppSettings["disable_timestamp_check"]).ToBool();
            if (!disable_timestamp_check)
            {
                #region 验证时间戳
                var timestamp = ConvertHelper.GetInt64(reqparams["timestamp"], -1);
                if (timestamp < 0)
                {
                    filterContext.Result = ResultHelper.BadRequest("缺少时间戳");
                    return;
                }
                var server_timestamp = DateTimeHelper.GetTimeStamp();
                //取绝对值
                if (Math.Abs(server_timestamp - timestamp) > Math.Abs(DeviationSeconds))
                {
                    filterContext.Result = ResultHelper.BadRequest("请求时间戳已经过期", new
                    {
                        client_timestamp = timestamp,
                        server_timestamp = server_timestamp
                    });
                    return;
                }
                #endregion
            }

            #region 验证签名
            var signKey = "sign";
            var sign = ConvertHelper.GetString(reqparams[signKey]).ToUpper();
            if (!ValidateHelper.IsAllPlumpString(sign))
            {
                filterContext.Result = ResultHelper.BadRequest("请求被拦截，获取不到签名");
                return;
            }

            //排序的字典
            var dict = new SortedDictionary<string, string>(new MyStringComparer());

            foreach (var p in reqparams.AllKeys)
            {
                if (!ValidateHelper.IsAllPlumpString(p) || p == signKey) { continue; }
                if (p.Length > 32 || reqparams[p]?.Length > 32) { continue; }

                dict[p] = ConvertHelper.GetString(reqparams[p]);
            }

            var strdata = dict.ToUrlParam();
            strdata += sign_key;
            strdata = strdata.ToLower();

            var md5 = strdata.ToMD5().ToUpper();
            if (sign != md5)
            {
                filterContext.Result = ResultHelper.BadRequest("签名错误", new
                {
                    client_sign = md5,
                    server_sign = sign,
                    server_order = strdata
                });
                return;
            }
            #endregion

            base.OnActionExecuting(filterContext);
        }
    }
}
