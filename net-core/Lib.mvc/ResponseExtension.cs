using Lib.helper;
using Microsoft.AspNetCore.Http;
using System.Configuration;

namespace Lib.mvc
{
    public static class ResponseExtension
    {
        public static void AllowCrossDomainAjax(this HttpContext context)
        {
            var Origin_Allow = ConvertHelper.GetString(ConfigurationManager.AppSettings["Origin_Allow"]).ToLower();
            if (!ValidateHelper.IsPlumpString(Origin_Allow)) { return; }
            //添加header实现跨域
            var Origin = ConvertHelper.GetString(context.Request.Headers["Origin"]);
            if (Origin.ToLower().IndexOf(Origin_Allow) >= 0)
            {
                context.Response.Headers["Access-Control-Allow-Origin"] = Origin;
                context.Response.Headers["Access-Control-Allow-Headers"] = "*, Origin, X-Requested-With, X_Requested_With, Content-Type, Accept";
                context.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
                context.Response.Headers["Access-Control-Allow-Credentials"] = "true";
            }
        }
    }
}
