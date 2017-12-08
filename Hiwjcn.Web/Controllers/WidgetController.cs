using Lib.mvc.attr;
using System.Web.Mvc;

namespace Hiwjcn.Web.Controllers
{
    [HideAction]
    public class WidgetController : WebCore.MvcLib.Controller.UserBaseController
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
