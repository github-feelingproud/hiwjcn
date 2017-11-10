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
    public static class HttpClientExtension
    {
        public static void AddFile_(this MultipartFormDataContent content, 
            string key, byte[] bs, string file_name, string content_type)
        {
            var fileContent = new ByteArrayContent(bs);
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = key,
                FileName = file_name,
                Size = bs.Length
            };
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(content_type);
            content.Add(fileContent, key);
        }

        public static Dictionary<string, string> ParamNotNull(this IDictionary<string, string> param)
        {
            return param.Where(x => ValidateHelper.IsPlumpString(x.Key))
                .ToDictionary(x => ConvertHelper.GetString(x.Key), x => ConvertHelper.GetString(x.Value));
        }

        public static string GetMethodString(this RequestMethodEnum m) => m.ToString();

    }
}
