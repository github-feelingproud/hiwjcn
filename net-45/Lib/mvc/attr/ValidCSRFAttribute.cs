using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Lib.mvc.attr
{
    /// <summary>
    /// 阻止CSRF
    /// </summary>
    public class ValidCSRFAttribute : ActionFilterAttribute
    {
        public static string CreateCSRFFieldHtml(string session_key)
        {
            var token = Com.GetUUID();
            System.Web.HttpContext.Current.Session[session_key] = token;
            var html = new StringBuilder();
            html.Append($"<input type='hidden' name='{FORM_KEY}' value='{token}' />");
            return html.ToString();
        }

        public const string FORM_KEY = "csrf_token";

        public string _SessionKey { get; set; }

        public ValidCSRFAttribute(string SessionKey)
        {
            this._SessionKey = SessionKey;

        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var token_client = filterContext.HttpContext.Request.Form[FORM_KEY];
            var token_server = filterContext.HttpContext.Session[_SessionKey]?.ToString();
            if (token_client != token_server)
            {
                filterContext.Result = new ContentResult() { Content = "try again" };
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
