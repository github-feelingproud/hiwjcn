using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Lib.core;
using Lib.mvc.attr;

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

        /// <summary>
        /// 生成下拉框html
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="value"></param>
        /// <param name="name"></param>
        /// <param name="selected_value"></param>
        /// <returns></returns>
        public static MvcHtmlString RenderOptions<T>(this List<T> list,
            Func<T, string> value, Func<T, string> name, string selected_value = null)
        {
            var html = new StringBuilder();
            foreach (var m in list)
            {
                var v = EncodingHelper.HtmlEncode(ConvertHelper.GetString(value(m)));
                var n = EncodingHelper.HtmlEncode(ConvertHelper.GetString(name(m)));
                if (v == selected_value)
                {
                    html.Append($"<option value=\"{v}\" selected=\"selected\">{n}</option>");
                }
                else
                {
                    html.Append($"<option value=\"{v}\">{n}</option>");
                }
            }
            return MvcHtmlString.Create(html.ToString());
        }

    }
}
