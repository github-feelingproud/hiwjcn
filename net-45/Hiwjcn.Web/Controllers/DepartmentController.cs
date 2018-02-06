using Hiwjcn.Framework;
using System.Web.Mvc;

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