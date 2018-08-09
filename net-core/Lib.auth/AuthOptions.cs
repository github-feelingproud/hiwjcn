using Lib.helper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.auth
{
    public class AuthOptions
    {
        public virtual string CookieDomain { get; set; }

        public virtual string TokenCookieKey { get; set; }

        public virtual TimeSpan? CookieExpired { get; set; }
    }

    public static class AuthOptionExtension
    {
        public static CookieOptions AsCookieOption(this AuthOptions data)
        {
            var option = new CookieOptions();
            if (ValidateHelper.IsPlumpString(data.CookieDomain))
            {
                option.Domain = data.CookieDomain;
            }
            if (data.CookieExpired != null)
            {
                option.MaxAge = data.CookieExpired.Value;
            }
            return option;
        }

        public static void Valid(this AuthOptions data)
        {
            throw new NotImplementedException();
        }
    }
}
