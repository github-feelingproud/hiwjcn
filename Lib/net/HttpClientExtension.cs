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
        public static ByteArrayContent CreateFileContent(this FileInfo f, string key, string contentType)
        {
            var fileContent = new ByteArrayContent(File.ReadAllBytes(f.FullName));
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = key,
                FileName = f.Name,
                Size = f.Length
            };
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return fileContent;
        }

        public static Dictionary<string, string> ParamNotNull(this IDictionary<string, string> param)
        {
            return param.Where(x => ValidateHelper.IsPlumpString(x.Key))
                .ToDictionary(x => ConvertHelper.GetString(x.Key), x => ConvertHelper.GetString(x.Value));
        }

        public static string GetMethodString(this RequestMethodEnum m) => m.ToString();

    }
}
