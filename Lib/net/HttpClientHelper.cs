using Lib.core;
using Lib.extension;
using Lib.helper;
using Lib.io;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Lib.net
{
    /// <summary>
    /// 请求方法
    /// </summary>
    public enum RequestMethodEnum : int
    {
        GET = 1, POST = 2, PUT = 3, DELETE = 4
    }

    public static class NetworkRequestExtension
    {
        public static string Get(this Uri url) => HttpClientHelper.Get(url.AbsoluteUri);

        public static async Task<string> GetAsync(this Uri url, int? second = null) =>
            await HttpClientHelper.GetAsync(url.AbsoluteUri, second);
    }

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
        public static Dictionary<string, string> NotNull(this Dictionary<string, string> param)
        {
            return param.ToDictionary(x => ConvertHelper.GetString(x.Key), x => ConvertHelper.GetString(x.Value));
        }

        private static string GetMethod(RequestMethodEnum m)
        {
            return m.ToString();
        }

        private static ByteArrayContent CreateFileContent(FileModel f, string key)
        {
            var fileContent = new ByteArrayContent(f.BytesArray);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = key,
                FileName = f.FileName,
                Size = f.Size
            };
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(f.ContentType);
            return fileContent;
        }

        /// <summary>
        /// 用的.NET4.5中httpclient的简易封装
        /// </summary>
        [Obsolete(nameof(HttpClient) + "有bug")]
        public static void SendHttpRequest(
            string url,
            Dictionary<string, string> param,
            Dictionary<string, FileModel> files,
            List<Cookie> cookies,
            RequestMethodEnum method,
            int timeout_second,
            VoidFunc<HttpResponseMessage> handler)
        {
            var t = SendHttpRequestAsync(url, param, files, cookies, method, timeout_second, handler);
            AsyncHelper.RunSync(() => t);
        }

        /// <summary>
        /// 用的.NET4.5中httpclient的简易封装
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <param name="files"></param>
        /// <param name="cookies"></param>
        /// <param name="method"></param>
        /// <param name="timeout_second"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        [Obsolete(nameof(HttpClient) + "有bug")]
        public static async Task SendHttpRequestAsync(
            string url,
            Dictionary<string, string> param,
            Dictionary<string, FileModel> files,
            List<Cookie> cookies,
            RequestMethodEnum method,
            int timeout_second,
            VoidFunc<HttpResponseMessage> handler)
        {
            var u = new Uri(url);
            using (var httpHandler = new HttpClientHandler() { UseCookies = false })
            {
                //创建cookie
                if (ValidateHelper.IsPlumpList(cookies))
                {
                    var cookieContainer = new System.Net.CookieContainer();
                    cookies.ForEach(x =>
                    {
                        cookieContainer.Add(u, x);
                    });
                    httpHandler.UseCookies = true;
                    httpHandler.CookieContainer = cookieContainer;
                }
                using (var client = new HttpClient(httpHandler))
                {
                    client.DefaultRequestHeaders.Add("UserAgent",
                        "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.95 Safari/537.11");
                    //发送X_FORWARDED_FOR头(若是用取源IP的方式，可以用这个来造假IP,对日志的记录无效)
                    client.DefaultRequestHeaders.Add("X_FORWARDED_FOR", "101.0.0.11");
                    client.Timeout = TimeSpan.FromSeconds(timeout_second);

                    HttpResponseMessage response = null;
                    if (method == RequestMethodEnum.POST)
                    {
                        //使用MultipartFormDataContent请参考邮件里的记录
                        //拼url传参
                        //using (var urlContent = new FormUrlEncodedContent(param)) { }
                        //form提交
                        using (var formContent = new MultipartFormDataContent())
                        {
                            if (ValidateHelper.IsPlumpDict(param))
                            {
                                param = param.NotNull();
                                foreach (var key in param.Keys)
                                {
                                    formContent.Add(new StringContent(param[key]), key);
                                }
                            }
                            if (ValidateHelper.IsPlumpDict(files))
                            {
                                foreach (var key in files.Keys)
                                {
                                    formContent.Add(CreateFileContent(files[key], key), key);
                                }
                            }
                            response = await client.PostAsync(u, formContent);
                        }
                    }
                    if (method == RequestMethodEnum.GET)
                    {
                        response = await client.GetAsync(u);
                    }
                    if (method == RequestMethodEnum.PUT)
                    {
                        throw new NotImplementedException();
                        //response = await client.PutAsync(u, null);
                    }
                    if (method == RequestMethodEnum.DELETE)
                    {
                        response = await client.DeleteAsync(u);
                    }

                    if (response == null) { throw new Exception("返回空"); }
                    using (response)
                    {
                        handler.Invoke(response);
                    }
                }
            }
        }

        /// <summary>
        /// 提交json，并返回字符串
        /// </summary>
        [Obsolete(nameof(HttpClient) + "有bug")]
        public static async Task<string> PostJsonAsync(string url, object jsonObj, int? timeout_second = null)
        {
            using (var client = new HttpClient())
            {
                if (timeout_second != null)
                {
                    client.Timeout = TimeSpan.FromSeconds(timeout_second.Value);
                }
                var response = await client.PostAsJsonAsync(url, jsonObj);
                return await response.Content.ReadAsStringAsync();
            }
        }

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
                        param = param.NotNull();
                        foreach (var key in param.Keys)
                        {
                            formContent.Add(new StringContent(param[key]), key);
                        }
                    }
                    var response = await client.PostAsync(u, formContent);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// 类似dapper传参
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <param name="timeout_second"></param>
        /// <returns></returns>
        [Obsolete(nameof(HttpClient) + "有bug")]
        public static async Task<string> PostAsync_(string url, object param, int? timeout_second = null)
        {
            return await PostAsync(url, Com.ObjectToStringDict(param), timeout_second);
        }

        /// <summary>
        /// 提交get请求
        /// </summary>
        [Obsolete(nameof(HttpClient) + "有bug")]
        public static async Task<string> GetAsync(string url, int? timeout_second = null)
        {
            var u = new Uri(url);
            using (var client = new HttpClient())
            {
                if (timeout_second != null)
                {
                    client.Timeout = TimeSpan.FromSeconds(timeout_second.Value);
                }
                var response = await client.GetAsync(u);
                return await response.Content.ReadAsStringAsync();
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
                data = Encoding.UTF8.GetBytes(param.NotNull().ToUrlParam());
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

        /// <summary>
        /// send request
        /// </summary>
        public static string Send(string url, byte[] data = null, string contentType = null,
            RequestMethodEnum method = RequestMethodEnum.POST, int? timeout_second = 30,
            int[] ensure_http_code = null)
        {
            HttpWebRequest req = null;
            HttpWebResponse res = null;
            try
            {
                //连接到目标网页
                req = (HttpWebRequest)WebRequest.Create(url);
                if (timeout_second != null)
                {
                    req.Timeout = timeout_second.Value * 1000;
                }
                req.Method = GetMethod(method);
                if (ValidateHelper.IsPlumpString(contentType))
                {
                    req.ContentType = contentType;
                }

                if (ValidateHelper.IsPlumpList(data))
                {
                    req.ContentLength = data.Length;
                    using (var stream = req.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }

                res = (HttpWebResponse)req.GetResponse();

                if (ValidateHelper.IsPlumpList(ensure_http_code) && !ensure_http_code.Contains((int)res.StatusCode))
                {
                    throw new Exception($"{url}请求的返回码：{(int)res.StatusCode}不在允许的范围内：{",".Join_(ensure_http_code)}");
                }

                using (var s = res.GetResponseStream())
                {
                    return ConvertHelper.StreamToString(s);
                }
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
        /// 处理请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <param name="cookies"></param>
        /// <param name="method"></param>
        /// <param name="timeout_second"></param>
        /// <param name="handler"></param>
        public static void HttpRequestHandler(
            string url,
            Dictionary<string, string> param,
            List<Cookie> cookies,
            RequestMethodEnum method,
            int timeout_second,
            VoidFunc<HttpWebResponse, HttpStatusCode> handler)
        {
            HttpWebRequest req = null;
            HttpWebResponse res = null;
            try
            {
                //连接到目标网页
                req = (HttpWebRequest)WebRequest.Create(url);
                req.Timeout = timeout_second * 1000;//10s请求超时
                req.Method = GetMethod(method);
                req.ContentType = "application/x-www-form-urlencoded";
                //添加cookie
                if (ValidateHelper.IsPlumpList(cookies))
                {
                    req.CookieContainer = new CookieContainer();
                    foreach (var c in cookies)
                    {
                        req.CookieContainer.Add(c);
                    }
                }
                //如果是post并且有参数
                if (method == RequestMethodEnum.POST && ValidateHelper.IsPlumpDict(param))
                {
                    param = param.NotNull();
                    var post_data = param.ToUrlParam();
                    var data = Encoding.UTF8.GetBytes(post_data);
                    using (var stream = req.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                res = (HttpWebResponse)req.GetResponse();
                handler.Invoke(res, res.StatusCode);
            }
            catch (Exception e)
            {
                e.AddErrorLog();
                throw e;
            }
            finally
            {
                try
                {
                    req?.Abort();
                }
                catch (Exception e)
                {
                    e.AddLog(typeof(HttpClientHelper));
                }
                res?.Dispose();
            }
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
            HttpRequestHandler(url, null, null, RequestMethodEnum.GET, 1000 * 60, (res, status) =>
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
            });
            return model;
        }

        /// <summary>
        /// 返回描述本地计算机上的网络接口的对象(网络接口也称为网络适配器)。
        /// </summary>
        /// <returns></returns>
        public static NetworkInterface[] NetCardInfo()
        {
            return NetworkInterface.GetAllNetworkInterfaces();
        }

        ///<summary>
        /// 通过NetworkInterface读取网卡Mac
        ///</summary>
        ///<returns></returns>
        public static List<string> GetMacByNetworkInterface()
        {
            List<string> macs = new List<string>();
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in interfaces)
            {
                macs.Add(ni.GetPhysicalAddress().ToString());
            }
            return macs;
        }

    }
}