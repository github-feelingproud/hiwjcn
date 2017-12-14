using Hiwjcn.Framework;
using Lib.mvc;
using System.Web;
using System.Web.Mvc;
using Lib.mvc.attr;

namespace Hiwjcn.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new LogErrorAttribute());
            filters.Add(new SetEncodingAttribute());
        }
    }
}
