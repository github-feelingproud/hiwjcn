using Hiwjcn.Framework;
using Lib.mvc.attr;
using System.Web.Mvc;

namespace Hiwjcn.Web.Controllers
{
    [HideAction]
    public class WidgetController :EpcBaseController
    {
        public WidgetController()
        {

        }

        [ChildActionOnly]
        public ActionResult Nav()
        {


            return PartialView();
        }

    }
}
