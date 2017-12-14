using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Lib.helper
{
    public static class EncodingHelper
    {
        public static string HtmlDecode(string s)
        {
            //System.Net.WebUtility.HtmlDecode(s);
            return HttpUtility.HtmlDecode(s);
        }

        public static string HtmlEncode(string s)
        {
            //System.Net.WebUtility.HtmlEncode(s);
            return HttpUtility.HtmlEncode(s);
        }

        public static string UrlDecode(string s)
        {
            return HttpUtility.UrlDecode(s);
        }

        public static string UrlEncode(string s)
        {
            return HttpUtility.UrlEncode(s);
        }
    }
}
