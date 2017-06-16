using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lib.mvc.attr;
using Lib.mvc;

namespace Hiwjcn.Web.Controllers
{
    [HideAction]
    public class UserApiController : BaseController
    {
        // GET: UserApi
        public ActionResult Index()
        {
            return GetJson(new { a = 5, b = 9 });
        }
    }
}