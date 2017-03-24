using Hiwjcn.Bll.Tag;
using System.Web.Mvc;
using WebLogic.Model.Tag;

namespace WebApp.Areas.Admin.Controllers
{
    public class TagController : WebCore.MvcLib.Controller.UserBaseController
    {
        public TagBll _TagBll = new TagBll();

        public ActionResult TagManage()
        {
            return RunActionWhenLogin((loginuser) =>
            {
                ViewData["list"] = _TagBll.GetList();
                return View();
            });
        }

        public ActionResult TagEdit(int? id)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                id = id ?? 0;
                if (id > 0)
                {
                    var model = _TagBll.GetTagByID(id.Value);
                    ViewData["model"] = model;
                }
                return View();
            });
        }

        [HttpPost]
        public ActionResult SaveTagAction(int? id, string name, string desc, string link)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var model = new TagModel();
                model.TagID = id ?? 0;
                model.TagName = name;
                model.TagDesc = desc;
                model.TagLink = link;
                var res = string.Empty;
                if (model.TagID > 0)
                {
                    res = _TagBll.UpdateTag(model);
                }
                else
                {
                    model.ItemCount = 0;
                    res = _TagBll.AddTag(model);
                }
                return GetJsonRes(res);
            });
        }

        [HttpPost]
        public ActionResult DeleteTagAction(int? id)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                id = id ?? 0;
                if (id <= 0) { return GetJsonRes("参数错误"); }
                var res = _TagBll.DeleteTag(id.Value);
                return GetJsonRes(res);
            });
        }

    }
}