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
        public static readonly IReadOnlyList<string> UserAgentsFactory = new List<string>()
        {
            "Mozilla/5.0 (Linux; U; Android 2.3.6; en-us; Nexus S Build/GRK39F) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "Avant Browser/1.2.789rel1 (http://www.avantbrowser.com)",
            "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/532.5 (KHTML, like Gecko) Chrome/4.0.249.0 Safari/532.5",
            "Mozilla/5.0 (Windows; U; Windows NT 5.2; en-US) AppleWebKit/532.9 (KHTML, like Gecko) Chrome/5.0.310.0 Safari/532.9",
            "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US) AppleWebKit/534.7 (KHTML, like Gecko) Chrome/7.0.514.0 Safari/534.7",
            "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US) AppleWebKit/534.14 (KHTML, like Gecko) Chrome/9.0.601.0 Safari/534.14",
            "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.14 (KHTML, like Gecko) Chrome/10.0.601.0 Safari/534.14",
            "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/534.20 (KHTML, like Gecko) Chrome/11.0.672.2 Safari/534.20",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/534.27 (KHTML, like Gecko) Chrome/12.0.712.0 Safari/534.27",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/13.0.782.24 Safari/535.1",
            "Mozilla/5.0 (Windows NT 6.0) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.120 Safari/535.2",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.7 (KHTML, like Gecko) Chrome/16.0.912.36 Safari/535.7",
            "Mozilla/5.0 (Windows; U; Windows NT 6.0 x64; en-US; rv:1.9pre) Gecko/2008072421 Minefield/3.0.2pre",
            "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.0.10) Gecko/2009042316 Firefox/3.0.10",
            "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-GB; rv:1.9.0.11) Gecko/2009060215 Firefox/3.0.11 (.NET CLR 3.5.30729)",
            "Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.1.6) Gecko/20091201 Firefox/3.5.6 GTB5",
            "Mozilla/5.0 (Windows; U; Windows NT 5.1; tr; rv:1.9.2.8) Gecko/20100722 Firefox/3.6.8 ( .NET CLR 3.5.30729; .NET4.0E)",
            "Mozilla/5.0 (Windows NT 6.1; rv:2.0.1) Gecko/20100101 Firefox/4.0.1",
            "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:2.0.1) Gecko/20100101 Firefox/4.0.1",
            "Mozilla/5.0 (Windows NT 5.1; rv:5.0) Gecko/20100101 Firefox/5.0",
            "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:6.0a2) Gecko/20110622 Firefox/6.0a2",
            "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:7.0.1) Gecko/20100101 Firefox/7.0.1",
            "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:2.0b4pre) Gecko/20100815 Minefield/4.0b4pre",
            "Mozilla/4.0 (compatible; MSIE 5.5; Windows NT 5.0 )",
            "Mozilla/4.0 (compatible; MSIE 5.5; Windows 98; Win 9x 4.90)",
            "Mozilla/5.0 (Windows; U; Windows XP) Gecko MultiZilla/1.6.1.0a",
            "Mozilla/2.02E (Win95; U)",
            "Mozilla/3.01Gold (Win95; I)",
            "Mozilla/4.8 [en] (Windows NT 5.1; U)",
            "Mozilla/5.0 (Windows; U; Win98; en-US; rv:1.4) Gecko Netscape/7.1 (ax)",
            "HTC_Dream Mozilla/5.0 (Linux; U; Android 1.5; en-ca; Build/CUPCAKE) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1",
            "Mozilla/5.0 (hp-tablet; Linux; hpwOS/3.0.2; U; de-DE) AppleWebKit/534.6 (KHTML, like Gecko) wOSBrowser/234.40.1 Safari/534.6 TouchPad/1.0",
            "Mozilla/5.0 (Linux; U; Android 1.5; en-us; sdk Build/CUPCAKE) AppleWebkit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1",
            "Mozilla/5.0 (Linux; U; Android 2.1; en-us; Nexus One Build/ERD62) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17",
            "Mozilla/5.0 (Linux; U; Android 2.2; en-us; Nexus One Build/FRF91) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "Mozilla/5.0 (Linux; U; Android 1.5; en-us; htc_bahamas Build/CRB17) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1",
            "Mozilla/5.0 (Linux; U; Android 2.1-update1; de-de; HTC Desire 1.19.161.5 Build/ERE27) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17",
            "Mozilla/5.0 (Linux; U; Android 2.2; en-us; Sprint APA9292KT Build/FRF91) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "Mozilla/5.0 (Linux; U; Android 1.5; de-ch; HTC Hero Build/CUPCAKE) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1",
            "Mozilla/5.0 (Linux; U; Android 2.2; en-us; ADR6300 Build/FRF91) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "Mozilla/5.0 (Linux; U; Android 2.1; en-us; HTC Legend Build/cupcake) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17",
            "Mozilla/5.0 (Linux; U; Android 1.5; de-de; HTC Magic Build/PLAT-RC33) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1 FirePHP/0.3",
            "Mozilla/5.0 (Linux; U; Android 1.6; en-us; HTC_TATTOO_A3288 Build/DRC79) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1",
            "Mozilla/5.0 (Linux; U; Android 1.0; en-us; dream) AppleWebKit/525.10  (KHTML, like Gecko) Version/3.0.4 Mobile Safari/523.12.2",
            "Mozilla/5.0 (Linux; U; Android 1.5; en-us; T-Mobile G1 Build/CRB43) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari 525.20.1",
            "Mozilla/5.0 (Linux; U; Android 1.5; en-gb; T-Mobile_G2_Touch Build/CUPCAKE) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1",
            "Mozilla/5.0 (Linux; U; Android 2.0; en-us; Droid Build/ESD20) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17",
            "Mozilla/5.0 (Linux; U; Android 2.2; en-us; Droid Build/FRG22D) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "Mozilla/5.0 (Linux; U; Android 2.0; en-us; Milestone Build/ SHOLS_U2_01.03.1) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17",
            "Mozilla/5.0 (Linux; U; Android 2.0.1; de-de; Milestone Build/SHOLS_U2_01.14.0) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17",
            "Mozilla/5.0 (Linux; U; Android 3.0; en-us; Xoom Build/HRI39) AppleWebKit/525.10  (KHTML, like Gecko) Version/3.0.4 Mobile Safari/523.12.2",
            "Mozilla/5.0 (Linux; U; Android 0.5; en-us) AppleWebKit/522  (KHTML, like Gecko) Safari/419.3",
            "Mozilla/5.0 (Linux; U; Android 1.1; en-gb; dream) AppleWebKit/525.10  (KHTML, like Gecko) Version/3.0.4 Mobile Safari/523.12.2",
            "Mozilla/5.0 (Linux; U; Android 2.0; en-us; Droid Build/ESD20) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17",
            "Mozilla/5.0 (Linux; U; Android 2.1; en-us; Nexus One Build/ERD62) AppleWebKit/530.17 (KHTML, like Gecko) Version/4.0 Mobile Safari/530.17",
            "Mozilla/5.0 (Linux; U; Android 2.2; en-us; Sprint APA9292KT Build/FRF91) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "Mozilla/5.0 (Linux; U; Android 2.2; en-us; ADR6300 Build/FRF91) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "Mozilla/5.0 (Linux; U; Android 2.2; en-ca; GT-P1000M Build/FROYO) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1",
            "Mozilla/5.0 (Linux; U; Android 3.0.1; fr-fr; A500 Build/HRI66) AppleWebKit/534.13 (KHTML, like Gecko) Version/4.0 Safari/534.13",
            "Mozilla/5.0 (Linux; U; Android 3.0; en-us; Xoom Build/HRI39) AppleWebKit/525.10  (KHTML, like Gecko) Version/3.0.4 Mobile Safari/523.12.2",
            "Mozilla/5.0 (Linux; U; Android 1.6; es-es; SonyEricssonX10i Build/R1FA016) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1",
            "Mozilla/5.0 (Linux; U; Android 1.6; en-us; SonyEricssonX10i Build/R1AA056) AppleWebKit/528.5  (KHTML, like Gecko) Version/3.1.2 Mobile Safari/525.20.1",
        }.AsReadOnly();

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
            Dictionary<string, string> files,
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
                                formContent.AddParam_(param);
                            }
                            if (ValidateHelper.IsPlumpDict(files))
                            {
                                foreach (var kv in files)
                                {
                                    formContent.AddFile_(kv.Key, kv.Value);
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
                        formContent.AddParam_(param);
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
}