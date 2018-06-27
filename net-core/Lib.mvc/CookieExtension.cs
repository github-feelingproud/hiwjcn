using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.mvc
{
    public static class CookieExtension
    {
        public static string GetCookie_(this IRequestCookieCollection cookie, string key, string deft = default(string))
            => cookie[key] ?? deft;
    }
}
