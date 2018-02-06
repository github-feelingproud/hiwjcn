using Hiwjcn.Bll.User;
using Hiwjcn.Core.Domain.User;
using Hiwjcn.Core.Domain;
using Hiwjcn.Framework;
using Lib.extension;
using Lib.mvc.auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Lib.mvc;
using WebCore.MvcLib.Controller;

namespace Hiwjcn.Web.Controllers
{
    public class DepartmentController : EpcBaseController
    {
        // GET: Department
        public ActionResult Index()
        {
            return View();
        }
    }
}