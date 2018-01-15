using Lib.helper;
using Lib.ioc;
using Lib.mvc;
using Lib.mvc.user;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System;
using Hiwjcn.Core;
using Lib.cache;

namespace WebCore.MvcLib.Controller
{
    [ValidateInput(false)]
    public class UserBaseController : BaseController
    {
        public readonly int PageSize = 10;

        public UserBaseController()
        {
            //
        }
    }
}
