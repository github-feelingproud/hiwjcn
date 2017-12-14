using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lib.mvc.user
{
    [Serializable]
    public class CheckLoginInfoData
    {
        public virtual bool success { get; set; }

        public virtual LoginUserInfo data { get; set; }

        public virtual string message { get; set; }
    }
}