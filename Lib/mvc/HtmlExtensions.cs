using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Lib.core;

namespace Lib.mvc
{
    public static class HtmlExtensions
    {
        /// <summary>
        /// 添加CSRF Token字段
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="session_key"></param>
        /// <returns></returns>
        public static MvcHtmlString CSRFTokenField(this System.Web.Mvc.HtmlHelper helper,
            string session_key)
        {
            return MvcHtmlString.Create(ValidCSRFAttribute.CreateCSRFFieldHtml(session_key));
        }

        /// <summary>
        /// 生成bootstrap分页
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="page"></param>
        /// <param name="pagecount"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static MvcHtmlString BootstrapPaging(this System.Web.Mvc.HtmlHelper helper,
            int page, int pagecount, string url)
        {
            var html = PagerHelper.GetPagerHtml(page, pagecount, url);
            return MvcHtmlString.Create(html);
        }

    }
}
