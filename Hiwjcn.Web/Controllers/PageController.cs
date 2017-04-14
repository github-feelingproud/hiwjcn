using Hiwjcn.Core.Infrastructure.Common;
using Hiwjcn.Core.Infrastructure.Page;
using Hiwjcn.Framework;
using Hiwjcn.Web.Models.Page;
using Lib.core;
using Lib.helper;
using Lib.mvc;
using System;
using System.Linq;
using System.Web.Mvc;
using WebApp.Models.Page;
using WebCore.MvcLib.Controller;
using Lib.mvc.attr;

namespace Hiwjcn.Web.Controllers
{
    public class PageController : WebCore.MvcLib.Controller.UserBaseController
    {
        private IPageService _IPageService { get; set; }
        private ILinkService _ILinkService { get; set; }

        /// <summary>
        /// 构造器
        /// </summary>
        public PageController(IPageService page, ILinkService link)
        {
            this._IPageService = page;
            this._ILinkService = link;
        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <returns></returns>
        [HtmlCache(3)]
        [LoadNavigation]
        public ActionResult Home()
        {
            return RunAction(() =>
            {
                var model = new HomeModel();

                var list = _ILinkService.GetTopLinks("home_link", 100);
                if (ValidateHelper.IsPlumpList(list))
                {
                    list = list.OrderBy(x => x.OrderNum).ToList();
                }
                model.HomeLinksList = list;

                ViewData["model"] = model;
                return View();
            });
        }

        [LoadNavigation]
        [Route("Page/Show/{name}/")]
        public ActionResult Show(string name)
        {
            return RunAction(() =>
            {
                if (!ValidateHelper.IsPlumpString(name))
                {
                    return Http404();
                }

                var model = new PageViewModel();
                model.Page = _IPageService.GetSection(name);
                if (model.Page == null)
                {
                    return Http404();
                }
                if (ValidateHelper.IsPlumpString(model.Page.RelGroup))
                {
                    model.PageList = _IPageService.GetSections(rel_group: model.Page.RelGroup);
                }
                model.CurrentPageName = name;
                ViewData["model"] = model;
                return View();
            });
        }

        /// <summary>
        /// 新闻
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [LoadNavigation]
        public ActionResult News(int? page)
        {
            return RunAction(() =>
            {
                page = CheckPage(page);
                var pagesize = 16;

                var data = _IPageService.GetSectionList(sectionType: "news", page: page.Value, pagesize: pagesize);
                if (data != null)
                {
                    ViewData["list"] = data.DataList;
                    ViewData["pager"] = data.GetPagerHtml("/page/news/", "page", page.Value, pagesize);
                }

                return View();
            });
        }

        /// <summary>
        /// 保持唤醒的请求地址
        /// </summary>
        /// <returns></returns>
        public ActionResult KeepAlive()
        {
            var alive_key = "alive";
            Session[alive_key] = DateTime.Now.ToString();
            return Content(ConvertHelper.GetString(Session[alive_key]));
        }

    }
}
