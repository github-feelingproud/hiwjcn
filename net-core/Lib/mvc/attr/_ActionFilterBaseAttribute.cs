using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lib.mvc.attr
{
    public abstract class _ActionFilterBaseAttribute : ActionFilterAttribute
    {
        protected IActionResult GetJson(object data) => new JsonResult(data);
    }
}
