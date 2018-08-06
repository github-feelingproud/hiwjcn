using System;

namespace Lib.ioc
{
    public class NotRegException : Exception
    {
        public NotRegException(string msg) : base(msg) { }
    }
}
