﻿using Lib.core;
using Lib.extension;
using Lib.helper;
using Lib.io;
using Lib.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Lib.net
{
    public class HttpClientManager : StaticClientManager<HttpClient>
    {
        public static readonly HttpClientManager Instance = new HttpClientManager();

        public override string DefaultKey => "default";

        public override bool CheckClient(HttpClient ins)
        {
            return ins != null;
        }

        public override HttpClient CreateNewClient(string key)
        {
            return new HttpClient();
        }

        public override void DisposeClient(HttpClient ins)
        {
            ins?.Dispose();
        }
    }

    /// <summary>
    /// http请求
    /// </summary>
    public static class HttpClientHelper
    {
        /// <summary>
        /// 提交post请求
        /// </summary>
        [Obsolete(nameof(HttpClient) + "有bug")]
        public static async Task<string> PostAsync(string url, Dictionary<string, string> param, int? timeout_second = null)
        {
            var u = new Uri(url);
            using (var client = new HttpClient())
            {
                if (timeout_second != null)
                {
                    client.Timeout = TimeSpan.FromSeconds(timeout_second.Value);
                }
                using (var formContent = new MultipartFormDataContent())
                {
                    if (ValidateHelper.IsPlumpDict(param))
                    {
                        formContent.AddParam_(param);
                    }
                    var response = await client.PostAsync(u, formContent);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// post json，测试成功
        /// </summary>
        /// <param name="url"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string PostJson(string url, string json)
        {
            var data = Encoding.UTF8.GetBytes(json);
            return Send(url, data, "application/json");
        }

        /// <summary>
        /// post json，测试成功
        /// </summary>
        /// <param name="url"></param>
        /// <param name="jsonObj"></param>
        /// <returns></returns>
        public static string PostJson(string url, object jsonObj)
        {
            return PostJson(url, (jsonObj ?? throw new ArgumentException("json对象为空")).ToJson());
        }

        /// <summary>
        /// post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string Post(string url, Dictionary<string, string> param)
        {
            var data = default(byte[]);
            if (ValidateHelper.IsPlumpDict(param))
            {
                data = Encoding.UTF8.GetBytes(param.ParamNotNull().ToUrlParam());
            }
            return Send(url, data, "application/x-www-form-urlencoded");
        }

        /// <summary>
        /// 类似dapper传参
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string Post_(string url, object param)
        {
            var data = default(Dictionary<string, string>);
            if (param != null)
            {
                data = Com.ObjectToStringDict(param);
            }
            return Post(url, data);
        }

        /// <summary>
        /// get
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string Get(string url)
        {
            return Send(url, null, string.Empty, RequestMethodEnum.GET);
        }

        public static void SendCore(string url, Action<HttpWebResponse, HttpStatusCode> handler,
            List<Cookie> cookies = null,
            byte[] data = null, string contentType = null, RequestMethodEnum method = RequestMethodEnum.POST,
            TimeSpan? timeout = null, int[] ensure_http_code = null)
        {
            HttpWebRequest req = null;
            HttpWebResponse res = null;
            try
            {
                //连接到目标网页
                req = (HttpWebRequest)WebRequest.Create(url);
                //timeout
                timeout = timeout ?? TimeSpan.FromSeconds(30);
                req.Timeout = (int)timeout.Value.TotalMilliseconds;
                //method
                req.Method = method.GetMethodString();
                //cookie
                if (ValidateHelper.IsPlumpList(cookies))
                {
                    req.CookieContainer = new CookieContainer();
                    foreach (var c in cookies)
                    {
                        req.CookieContainer.Add(c);
                    }
                }
                //content type
                if (ValidateHelper.IsPlumpString(contentType))
                {
                    req.ContentType = contentType;
                }
                //data
                if (ValidateHelper.IsPlumpList(data))
                {
                    req.ContentLength = data.Length;
                    using (var stream = req.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                //response
                res = (HttpWebResponse)req.GetResponse();
                //ensure http status
                if (ValidateHelper.IsPlumpList(ensure_http_code) && !ensure_http_code.Contains((int)res.StatusCode))
                {
                    throw new Exception($"{url}请求的返回码：{(int)res.StatusCode}不在允许的范围内：{",".Join_(ensure_http_code)}");
                }

                //callback
                handler.Invoke(res, res.StatusCode);

            }
            finally
            {
                try
                {
                    req?.Abort();
                }
                catch (Exception e)
                {
                    e.AddErrorLog();
                }
                res?.Dispose();
            }
        }

        /// <summary>
        /// send request
        /// </summary>
        public static string Send(string url, byte[] data = null, string contentType = null,
            RequestMethodEnum method = RequestMethodEnum.POST, int? timeout_second = 30,
            int[] ensure_http_code = null)
        {
            var response_data = string.Empty;
            Action<HttpWebResponse, HttpStatusCode> callback = (res, code) =>
            {
                using (var s = res.GetResponseStream())
                {
                    response_data = ConvertHelper.StreamToString(s);
                }
            };
            TimeSpan? to = null;
            if (timeout_second != null)
            {
                to = TimeSpan.FromSeconds(timeout_second.Value);
            }
            SendCore(url, callback, data: data, contentType: contentType, method: method, timeout: to, ensure_http_code: ensure_http_code);

            return response_data;
        }

        /// <summary>
        /// 通过contentTYPE获取文件格式
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetExtByContentType(string t)
        {
            t = ConvertHelper.GetString(t).ToLower();
            if (t.Contains("jpg") || t.Contains("jpeg")) { return ".jpg"; }
            if (t.Contains("png")) { return ".png"; }
            if (t.Contains("gif")) { return ".gif"; }
            if (t.Contains("bmp")) { return ".bmp"; }
            if (t.Contains("mp3")) { return ".mp3"; }
            if (t.Contains("mp4")) { return ".mp4"; }
            return string.Empty;
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="save_path"></param>
        /// <returns></returns>
        public static UpLoadFileResult DownloadFile(string url, string save_path)
        {
            var model = new UpLoadFileResult();
            Action<HttpWebResponse, HttpStatusCode> callback = (res, status) =>
            {
                if (status != HttpStatusCode.OK) { throw new Exception($"status:{status}"); }
                using (var s = res.GetResponseStream())
                {
                    IOHelper.CreatePathIfNotExist(save_path);
                    model.SuccessPreparePath = true;
                    var ext = string.Empty;
                    var sp = url.Split('.');
                    if (sp.Length > 1)
                    {
                        ext = "." + sp[sp.Length - 1];
                    }
                    if (!ValidateHelper.IsPlumpString(ext))
                    {
                        ext = GetExtByContentType(res.ContentType);
                    }
                    var file_path = Path.Combine(save_path, Com.GetUUID() + ext);
                    using (var fs = new FileStream(file_path, FileMode.Create, FileAccess.Write))
                    {
                        var len = 0;
                        var bs = new byte[1024];
                        while ((len = s.Read(bs, 0, bs.Length)) > 0)
                        {
                            fs.Write(bs, 0, len);
                        }
                        fs.Flush();

                        var fileinfo = new FileInfo(file_path);
                        model.OriginName = model.FileName = fileinfo.Name;
                        model.WebPath = "/";
                        model.FileExtension = fileinfo.Extension;
                        model.DirectoryPath = fileinfo.Directory?.FullName;
                        model.FilePath = fileinfo.FullName;
                        model.FileSize = (int)fileinfo.Length;
                        model.Info = fileinfo.LastWriteTime.ToString();

                        model.SuccessUpload = true;
                    }
                }
            };

            SendCore(url, callback, method: RequestMethodEnum.GET, timeout: TimeSpan.FromSeconds(60));

            return model;
        }

        /// <summary>
        /// 返回描述本地计算机上的网络接口的对象(网络接口也称为网络适配器)。
        /// </summary>
        /// <returns></returns>
        public static NetworkInterface[] NetCardInfo() => NetworkInterface.GetAllNetworkInterfaces();

        ///<summary>
        /// 通过NetworkInterface读取网卡Mac
        ///</summary>
        ///<returns></returns>
        public static List<string> GetMacByNetworkInterface() =>
            NetCardInfo().Select(x => x.GetPhysicalAddress()?.ToString()).Where(x => ValidateHelper.IsPlumpString(x)).ToList();

    }

    public sealed class JsonContent : StringContent
    {
        private const string MediaType = "application/json";

        public JsonContent(object value, Encoding encoding = null) :
            base(
                (value ?? throw new ArgumentNullException(nameof(value))).ToJson(), 
                encoding ?? Encoding.UTF8, 
                MediaType)
        {
            //
        }
    }
}