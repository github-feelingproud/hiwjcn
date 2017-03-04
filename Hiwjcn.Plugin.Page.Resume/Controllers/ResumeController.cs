using Hiwjcn.Framework.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;

namespace Hiwjcn.Plugin.Page.Resume.Controllers
{
    public class ResumeController : Controller
    {
        public ActionResult Show()
        {
            return View("~/App_Data/Plugins/Page.Resume/Views/Resume/Show.cshtml");
        }
    }
}
