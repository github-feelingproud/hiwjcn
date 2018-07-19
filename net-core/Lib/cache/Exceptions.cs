using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.cache
{
    public class CacheException : Exception
    {
        public CacheException(string msg) : base(msg) { }
    }
}
