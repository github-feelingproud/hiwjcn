using Bll.Category;
using System.Web.Mvc;

namespace Hiwjcn.Web.Controllers
{
    public class WidgetController : WebCore.MvcLib.Controller.UserController
    {
        private CategoryBll _CategoryBll { get; set; }
        public WidgetController()
        {
            this._CategoryBll = new CategoryBll();
        }

        [ChildActionOnly]
        public ActionResult Nav()
        {
            ViewData["nav_list"] = _CategoryBll.GetCategoryByType("nav_list", maxCount: 500);

            return PartialView();
        }

    }
}
