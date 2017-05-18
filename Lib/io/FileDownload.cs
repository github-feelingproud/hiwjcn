using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Collections;

namespace Lib.io
{
    /// <summary>
    ///FileDownload 的摘要说明
    /// </summary>
    public class FileDownload
    {
        private string filepath = string.Empty;
        private HttpResponse response = null;
        public FileDownload(HttpResponse res, string path)
        {
            this.response = res;
            this.filepath = path;
        }
        /// <summary>
        /// 下载
        /// </summary>
        public void Download()
        {
            var fi = new FileInfo(filepath);
            if (!fi.Exists)
            {
                throw new Exception("文件不存在");
            }
            response.ContentType = "application/octet-stream";
            response.AddHeader("Content-Disposition", "attachment; filename=" + fi.Name);
            using (var fs = fi.OpenRead())
            {
                var b = new byte[1024 * 200];
                int i = 0;
                while ((i = fs.Read(b, 0, b.Length)) > 0)
                {
                    response.OutputStream.Write(b, 0, i);
                    response.Flush();
                }
            }
        }
    }
}
