using Autofac.Extras.DynamicProxy;
using Dal.Sys;
using Hiwjcn.Bll;
using Hiwjcn.Core.Infrastructure.Common;
using Lib.helper;
using Lib.core;
using Model.Sys;
using System.Collections.Generic;
using System.Linq;

namespace Bll.Sys
{
    /// <summary>
    /// 链接
    /// </summary>
    [Intercept(typeof(AopLogError))]
    public class LinkService : ServiceBase<LinkModel>, ILinkService
    {
        private LinkDal _LinkDal { get; set; }
        public LinkService()
        {
            this._LinkDal = new LinkDal();
        }

        public override string CheckModel(LinkModel model)
        {
            if (model == null) { return "对象为空"; }
            if (!ValidateHelper.IsPlumpString(model.Name))
            {
                return "锚文本不能为空";
            }
            if (!ValidateHelper.IsPlumpString(model.Url))
            {
                return "链接地址不能为空";
            }
            if (!ValidateHelper.IsPlumpString(model.Target))
            {
                return "Target不能为空";
            }
            if (model.UserID <= 0)
            {
                return "用户不能为空";
            }
            if (!ValidateHelper.IsPlumpString(model.LinkType))
            {
                return "连接类型不能为空";
            }
            return string.Empty;
        }

        /// <summary>
        /// 根据ID获取连接
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LinkModel GetLinkByID(int id)
        {
            string key = Com.GetCacheKey("linkbll.GetLinksByIDS", id.ToString());
            return Cache(key, () =>
            {
                return _LinkDal.GetFirst(x => x.LinkID == id);
            });
        }

        /// <summary>
        /// 读取链接
        /// </summary>
        /// <param name="link_type"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public virtual List<LinkModel> GetTopLinks(string link_type, int count = 99999)
        {
            if (!ValidateHelper.IsPlumpString(link_type) || count <= 0)
            {
                return null;
            }
            string key = GetCacheKey("linkbll.gettoplinks", link_type, count.ToString());
            return Cache(key, () =>
            {
                var list = _LinkDal.QueryList(
                    where: x => x.LinkType == link_type,
                    orderby: x => x.OrderNum, Desc: true,
                    start: 0, count: count);
                return list;
            });
        }

        /// <summary>
        /// 删除链接
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string DeleteLink(int id)
        {
            var model = _LinkDal.GetFirst(x => x.LinkID == id);
            if (model == null) { return "记录不存在"; }
            return _LinkDal.Delete(model) > 0 ? SUCCESS : "删除失败";
        }

        /// <summary>
        /// 添加链接
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AddLink(LinkModel model)
        {
            string check = CheckModel(model);
            if (ValidateHelper.IsPlumpString(check))
            {
                return check;
            }
            return _LinkDal.Add(model) > 0 ? SUCCESS : "添加失败";
        }

        /// <summary>
        /// 更新链接
        /// </summary>
        /// <param name="updatemodel"></param>
        /// <returns></returns>
        public string UpdateLink(LinkModel updatemodel)
        {
            var model = _LinkDal.GetByKeys(updatemodel.LinkID);
            if (model == null) { return "链接不存在"; }
            model.Image = updatemodel.Image;
            model.Name = updatemodel.Name;
            model.OrderNum = updatemodel.OrderNum;
            model.Target = updatemodel.Target;
            model.Title = updatemodel.Title;
            model.Url = updatemodel.Url;

            string errinfo = CheckModel(model);
            if (ValidateHelper.IsPlumpString(errinfo)) { return errinfo; }
            return _LinkDal.Update(model) > 0 ? SUCCESS : "更新链接失败";
        }
    }
}
