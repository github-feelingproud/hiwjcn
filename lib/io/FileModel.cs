using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lib.io
{
    public class FileModel
    {
        public string FileName { get; set; }

        public string FullName { get; set; }

        public string RelativePath { get; set; }

        public long Size { get; set; }

        public string Extension { get; set; }

        public DateTime LastAccessTime { get; set; }

        public DateTime LastWriteTime { get; set; }

        public bool IsDir { get; set; }

        public byte[] BytesArray { get; set; }

        public string ContentType { get; set; }
    }
}
