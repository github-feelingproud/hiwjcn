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
        public static void CheckSSOConfig()
        {
            var list = new List<string>();
            var config = ConfigHelper.Instance;

            if (!ValidateHelper.IsPlumpString(config.CallBackUrl))
            {
                list.Add(nameof(config.CallBackUrl));
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

    }
}
