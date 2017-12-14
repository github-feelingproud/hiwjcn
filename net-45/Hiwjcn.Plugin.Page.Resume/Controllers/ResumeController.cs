using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;
using Lib.mvc.plugin;
using Lib.core;

namespace Hiwjcn.Plugin.Page.Resume.Controllers
{
    public class ResumeController : BasePluginController
    {
        public ISettings _ISettings { get; private set; }

        public ResumeController(ISettings setting)
        {
            this._ISettings = setting;
        }

        public ActionResult Show()
        {
            return View("~/App_Data/Plugins/Page.Resume/Views/Resume/Show.cshtml");
        }
    }
}
