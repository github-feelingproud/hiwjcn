using Lib.core;
using Lib.extension;
using Lib.helper;
using Lib.net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Lib.mvc.user
{
    /*
     
    <!--SSO登录地址-->
    <add key="SSOLoginUrl" value="http://localhost:51735/sso/login/" />
    <!--SSO退出地址-->
    <add key="SSOLogoutUrl" value="http://localhost:51735/sso/logout/" />
    <!--SSO中用来检查登录信息的接口-->
    <add key="CheckLoginInfoUrl" value="http://sso.qipeilong.cn/sso/CheckLoginInfo/" />
    <!--在SSO中配置的tokenID-->
    <add key="WebToken" value="***" />
    <!--默认跳转地址-->
    <add key="DefaultRedirectUrl" value="http://sso.qipeilong.cn/user/userlist/" />
    <!--指定跳回地址-->
    <add key="SSO_CONTINUE_URL" value="" />
    <!--指定服务器跳转，还是把跳转地址返回给前端跳转-->
    <add key="SSO_NO_LOGIN_RESULT_FOR_INTERFACE" value="" />

         */

    /// <summary>
    /// sso
    /// </summary>
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
        public static async Task<CheckLoginInfoData> GetCheckTokenResult(string uid, string token)
        {
            CheckSSOConfig();

            if (!ValidateHelper.IsAllPlumpString(uid, token)) { return null; }

            var checkUrl = ConfigHelper.Instance.CheckLoginInfoUrl;

            var dict = new Dictionary<string, string>();
            dict["uid"] = uid;
            dict["token"] = token;

            CheckLoginInfoData info = null;

            var json = await HttpClientHelper.PostAsync(checkUrl, dict, 15);
            if (ValidateHelper.IsPlumpString(json))
            {
                info = json.JsonToEntity<CheckLoginInfoData>();
            }

            return info;
        }

        /// <summary>
        /// 登录回调
        /// </summary>
        public static async Task<ActionResult> GetCallBackResult(string url, string uid, string token, LoginStatus loginstatus)
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
                    loginstatus.SetUserLogin(loginuser: data.data);
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
