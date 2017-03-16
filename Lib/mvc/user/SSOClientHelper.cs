using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Http;
using Lib.helper;
using Lib.core;
using Lib.net;

namespace Lib.mvc.user
{
    public static class SSOClientHelper
    {
        /// <summary>
        /// 检查单点登录配置
        /// </summary>
        public static void CheckSSOConfig()
        {
            var list = new List<string>();
            var config = ConfigHelper.Instance;

            if (!ValidateHelper.IsPlumpString(config.WebToken))
            {
                list.Add(nameof(config.WebToken));
            }
            if (!ValidateHelper.IsPlumpString(config.SSOLoginUrl))
            {
                list.Add(nameof(config.SSOLoginUrl));
            }
            if (!ValidateHelper.IsPlumpString(config.SSOLogoutUrl))
            {
                list.Add(nameof(config.SSOLogoutUrl));
            }
            if (!ValidateHelper.IsPlumpString(config.CheckLoginInfoUrl))
            {
                list.Add(nameof(config.CheckLoginInfoUrl));
            }
            if (!ValidateHelper.IsPlumpString(config.DefaultRedirectUrl))
            {
                list.Add(nameof(config.DefaultRedirectUrl));
            }

            if (list.Count > 0)
            {
                throw new Exception($"请配置{string.Join(",", list)}");
            }
        }

        /// <summary>
        /// 这个是在client端执行
        /// </summary>
        /// <param name="url"></param>
        /// <param name="uid"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<CheckLoginInfoData> GetCheckTokenResult(string uid, string token)
        {
            CheckSSOConfig();

            if (!ValidateHelper.IsAllPlumpString(uid, token)) { return null; }

            var checkUrl = ConfigHelper.Instance.CheckLoginInfoUrl;

            var dict = new Dictionary<string, string>();
            dict["uid"] = uid;
            dict["token"] = token;

            CheckLoginInfoData info = null;

            await HttpClientHelper.SendHttpRequestAsync(checkUrl, dict, null, null, RequestMethodEnum.POST, 10, async (res) =>
            {
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    if (ValidateHelper.IsAllPlumpString(json))
                    {
                        info = JsonHelper.JsonToEntity<CheckLoginInfoData>(json);
                    }
                }
            });

            return info;
        }

        /// <summary>
        /// 登录回调
        /// </summary>
        /// <param name="url"></param>
        /// <param name="uid"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<ActionResult> GetCallBackResult(string url, string uid, string token)
        {
            var redirect_url = url;
            var data = await GetCheckTokenResult(uid, token);
            if (data == null)
            {
                return new ContentResult() { Content = "返回数据无法解析" };
            }
            if (data.success)
            {
                if (data.data != null)
                {
                    AccountHelper.User.SetUserLogin(loginuser: data.data);
                    return new RedirectResult(redirect_url);
                }
                else
                {
                    return new ContentResult() { Content = "服务器返回了错误的数据" };
                }
            }
            else
            {
                return new ContentResult() { Content = data.message };
            }
        }

        /// <summary>
        /// 构造SSO登录地址
        /// </summary>
        /// <param name="current_url"></param>
        /// <returns></returns>
        public static string BuildSSOLoginUrl(string current_url)
        {
            var config = ConfigHelper.Instance;
            current_url = EncodingHelper.UrlEncode(current_url);
            return $"{config.SSOLoginUrl}?url={current_url}&web_token={config.WebToken}";
        }
    }
}
