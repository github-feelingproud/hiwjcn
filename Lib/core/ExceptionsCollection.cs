using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.core
{
    [Serializable]
    public class MsgException : Exception
    {
        public MsgException(string msg) : base(msg)
        { }
    }

    [Serializable]
    public class SourceNotExistException : Exception
    { }
}
