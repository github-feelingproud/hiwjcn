using Lib.helper;
using Microsoft.AspNetCore.Http;
using Lib.ioc;

namespace Lib.mvc
{
    public static class ResponseExtension
    {
        public static void AllowCrossDomainAjax(this HttpContext context)
        {
            using (var s = IocContext.Instance.Scope())
            {
                var config = s.ResolveConfig_();

                var Origin_Allow = ConvertHelper.GetString(config["Origin_Allow"]).ToLower();
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
}
