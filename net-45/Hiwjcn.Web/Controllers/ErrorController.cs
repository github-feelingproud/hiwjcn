using Lib.mvc;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Hiwjcn.Web.Controllers
{
    /// <summary>
    /// 不要继承basecontroller
    /// </summary>
    public class ErrorController : Controller
    {

        public ActionResult Http404() => new Http404();

        public ActionResult Http403() => new Http403();

        public ActionResult Http500() => new Http500();

    }
}
