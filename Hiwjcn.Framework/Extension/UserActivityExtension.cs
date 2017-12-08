using Hiwjcn.Core.Domain.Sys;
using Lib.mvc;
using System.Web;
using System.Web.Mvc;

namespace Hiwjcn.Framework.Extension
{
    public static class UserActivityExtension
    {
        public static void FromHttpContext(this UserActivityEntity model, HttpContext context)
        { }

        public static void FromController(this UserActivityEntity model, Controller controller)
        {
            model.FromHttpContext(HttpContext.Current);
            var data = controller.RouteData.GetA_C_A();
            model.ActionName = data.action;
            model.ControllerName = data.controller;
            model.ActionName = data.action;
        }
    }
}
