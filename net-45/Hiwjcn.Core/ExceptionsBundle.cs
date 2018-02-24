using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiwjcn.Core
{
    public class NoOrgException : Exception
    {
        public NoOrgException() : base("没有加入组织")
        {
            //
        }
    }

    public class NoPermissionInOrgException : Exception
    {
        public NoPermissionInOrgException() : base("没有许可进行操作")
        {
            //
        }
    }

    public class NoLoginException : Exception
    {
        public NoLoginException() : base("没有登录")
        {
            //
        }
    }
}
