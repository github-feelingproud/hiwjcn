using Lib.core;
using Lib.io;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Net.Sockets;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using Lib.extension;
using Lib.helper;

namespace Lib.net
{
    /// <summary>
    /// 请求方法
    /// </summary>
    public enum RequestMethodEnum : int
    {
        GET = 1, POST = 2, PUT = 3, DELETE = 4
    }

    /// <summary>
    /// http请求
    /// </summary>
    public static class HttpClientHelper
    {
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
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <param name="cookies"></param>
        /// <param name="method"></param>
        /// <param name="timeout_second"></param>
        /// <param name="handler"></param>
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
                        if (param == null) { param = new Dictionary<string, string>(); }
                        //拼url传参
                        //using (var urlContent = new FormUrlEncodedContent(param)) { }
                        //form提交
                        using (var formContent = new MultipartFormDataContent())
                        {
                            if (ValidateHelper.IsPlumpDict(param))
                            {
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

        //浏览器列表
        public static readonly string[] BROWSER_LIST = new string[] { "ie", "chrome", "mozilla", "netscape", "firefox", "opera" };
        //搜索引擎列表
        public static readonly string[] SEARCH_ENGINE_LIST = new string[] { "baidu", "google", "360", "sogou", "bing", "yahoo" };

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
                //发送X_FORWARDED_FOR头(若是用取源IP的方式，可以用这个来造假IP,对日志的记录无效)
                req.Headers.Add("X_FORWARDED_FOR", "101.0.0.11");
                req.Timeout = timeout_second * 1000;//10s请求超时
                req.Method = GetMethod(method);
                req.KeepAlive = true;
                req.AllowAutoRedirect = true;
                req.ContentType = "application/x-www-form-urlencoded";
                req.AllowAutoRedirect = true;
                req.Accept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, */*";
                req.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322)";
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
                    var post_data = Com.DictToUrlParams(param);
                    var data = Encoding.UTF8.GetBytes(post_data);
                    using (var stream = req.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                res = (HttpWebResponse)req.GetResponse();
                handler.Invoke(res, res.StatusCode);
            }
            catch (Exception e) //遇到错误，打印错误
            {
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
        /// 读取字符串
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <param name="cookies"></param>
        /// <param name="method"></param>
        /// <param name="timeout_second"></param>
        /// <returns></returns>
        public static string RequestString(string url,
            Dictionary<string, string> param,
            List<Cookie> cookies,
            RequestMethodEnum method,
            int timeout_second)
        {
            var str = string.Empty;
            HttpRequestHandler(url, param, cookies, method, timeout_second, (res, status) =>
            {
                if (status != HttpStatusCode.OK) { throw new Exception($"status:{status}"); }
                using (var s = res.GetResponseStream())
                {
                    using (var reader = new StreamReader(s))
                    {
                        str = reader.ReadToEnd();
                    }
                }
            });
            return str;
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