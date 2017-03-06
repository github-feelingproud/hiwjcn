using Lib.helper;
using Lib.net;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Lib.api
{
    public static class UmengPushHelper
    {

        public static readonly string UMENG_PUSH_URL = ConfigurationManager.AppSettings[nameof(UMENG_PUSH_URL)] ?? "http://msg.umeng.com/api/send";

        public static readonly string UMENG_PUSH_APP_KEY_ANDROID = ConfigurationManager.AppSettings[nameof(UMENG_PUSH_APP_KEY_ANDROID)];
        public static readonly string UMENG_PUSH_APP_MASTER_SECRET_KEY_ANDROID = ConfigurationManager.AppSettings[nameof(UMENG_PUSH_APP_MASTER_SECRET_KEY_ANDROID)];

        public static readonly string UMENG_PUSH_APP_KEY_IOS = ConfigurationManager.AppSettings[nameof(UMENG_PUSH_APP_KEY_IOS)];
        public static readonly string UMENG_PUSH_APP_MASTER_SECRET_KEY_IOS = ConfigurationManager.AppSettings[nameof(UMENG_PUSH_APP_MASTER_SECRET_KEY_IOS)];

        public static string BuildSign(string post_body, string SK)
        {
            var METHOD = "POST";
            var str = $"{METHOD}{UMENG_PUSH_URL}{post_body}{SK}";
            return SecureHelper.GetMD5(str).ToLower();
        }

        public static async Task<UmengReturn> Send(object post_body, string SK)
        {
            var data_json = JsonHelper.ObjectToJson(post_body);
            var sign = BuildSign(data_json, SK);
            var url = $"{UMENG_PUSH_URL}?sign={sign}";

            var json = await HttpClientHelper.PostJson(url, post_body);

            return UmengReturn.FromJson(json);
        }

        public static async Task<UmengReturn> PushAndroid(PayLoadAndroid payload, List<string> device_tokens)
        {
            if (!ValidateHelper.IsAllPlumpString(UMENG_PUSH_APP_KEY_ANDROID, UMENG_PUSH_APP_MASTER_SECRET_KEY_ANDROID, UMENG_PUSH_URL))
            {
                throw new Exception("安卓推送配置错误");
            }
            var data = new
            {
                appkey = UMENG_PUSH_APP_KEY_ANDROID,
                timestamp = DateTimeHelper.GetTimeStamp().ToString(),
                alias_type = "qpl",
                type = "customizedcast",
                alias = string.Join(",", device_tokens),
                payload = payload
            };
            return await Send(data, UMENG_PUSH_APP_MASTER_SECRET_KEY_ANDROID);
        }
        //public static async Task<UmengReturn> Push(PayLoadIOS payload, List<string> device_tokens)
        //{
        //    if (!ValidateHelper.IsAllPlumpString(UMENG_PUSH_APP_KEY_IOS, UMENG_PUSH_APP_MASTER_SECRET_KEY_IOS, UMENG_PUSH_URL))
        //    {
        //        throw new Exception("IOS推送配置错误");
        //    }
        //    var data = new
        //    {
        //        appkey = UMENG_PUSH_APP_KEY_IOS,
        //        timestamp = DateTimeHelper.GetTimeStamp().ToString(),
        //        alias_type = "qpl",
        //        type = "customizedcast",
        //        alias = string.Join(",", device_tokens),
        //        payload = payload
        //    };
        //    return await Send(data, UMENG_PUSH_APP_MASTER_SECRET_KEY_IOS);
        //}
        public static async Task<UmengReturn> PushIOS(object payload, List<string> device_tokens)
        {
            if (!ValidateHelper.IsAllPlumpString(UMENG_PUSH_APP_KEY_IOS, UMENG_PUSH_APP_MASTER_SECRET_KEY_IOS, UMENG_PUSH_URL))
            {
                throw new Exception("IOS推送配置错误");
            }
            var data = new
            {
                appkey = UMENG_PUSH_APP_KEY_IOS,
                timestamp = DateTimeHelper.GetTimeStamp().ToString(),
                alias_type = "qpl",
                type = "customizedcast",
                alias = string.Join(",", device_tokens),
                payload = payload
            };
            return await Send(data, UMENG_PUSH_APP_MASTER_SECRET_KEY_IOS);
        }
    }

    #region Android
    public class PayLoadAndroid
    {
        public virtual string display_type { get { return "notification"; } }
        public virtual PayLoadAndroidBody body { get; set; }
        public virtual object extra { get; set; }
    }
    public class PayLoadAndroidBody
    {
        public virtual string ticker { get; set; }
        public virtual string title { get; set; }
        public virtual string text { get; set; }
        public virtual string after_open { get { return "go_custom"; } }
    }
    #endregion

    #region IOS
    //public class PayLoadIOS
    //{
    //    public virtual PayLoadIOSAPS aps { get; set; }
    //}
    //public class PayLoadIOSAPS
    //{
    //    public virtual string alert { get; set; }
    //}
    #endregion

    /// <summary>
    /// 返回数据
    /// </summary>
    public class UmengReturn
    {
        public static UmengReturn FromJson(string json)
        {
            return JsonHelper.JsonToEntity<UmengReturn>(json);
        }
        public virtual string ret { get; set; }
        public virtual UmengReturnData data { get; set; }
        public class UmengReturnData
        {
            public virtual string msg_id { get; set; }
            public virtual string task_id { get; set; }
            public virtual string error_code { get; set; }
            public virtual string thirdparty_id { get; set; }
        }
    }
}
