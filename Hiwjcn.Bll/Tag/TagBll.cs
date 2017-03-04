using Bll;
using Hiwjcn.Dal.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebLogic.Model.Tag;
using Lib.helper;
using Lib.core;

namespace Hiwjcn.Bll.Tag
{
    public class TagBll : ServiceBase<TagModel>
    {
        public TagBll()
        {
            //
        }

        public override string CheckModel(TagModel model)
        {
            if (model == null) { return "对象为空"; }
            if (!ValidateHelper.IsPlumpString(model.TagName)) { return "标签名为空"; }
            if (!ValidateHelper.IsLenInRange(model.TagName, 1, 20)) { return "标签名长度为1~20"; }
            return string.Empty;
        }

        /// <summary>
        /// 通过id获取标签
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TagModel GetTagByID(int id)
        {
            if (id <= 0) { return null; }
            var list = GetList(new List<int>() { id }, take: 1);
            if (ValidateHelper.IsPlumpList(list))
            {
                return list[0];
            }
            return null;
        }

        /// <summary>
        /// 获取标签
        /// </summary>
        /// <param name="tagtype"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public List<TagModel> GetList(List<int> ids = null, int skip = 0, int take = 1000)
        {
            ids = ConvertHelper.NotNullList(ids).Where(x => x > 0).ToList();
            var key = Com.GetCacheKey("TagBll.GetList:", string.Join(",", ids), skip.ToString(), take.ToString());

            return Cache(key, () =>
            {
                var tagdal = new TagDal();
                List<TagModel> list = null;
                tagdal.PrepareIQueryable((query) =>
                {
                    if (ValidateHelper.IsPlumpList(ids))
                    {
                        query = query.Where(x => ids.Contains(x.TagID));
                    }
                    list = query.OrderByDescending(x => x.TagID).Skip(skip).Take(take).ToList();
                    return true;
                });
                return list;
            });
        }

        /// <summary>
        /// 添加标签
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AddTag(TagModel model)
        {
            string err = CheckModel(model);
            if (ValidateHelper.IsPlumpString(err)) { return err; }
            var tagdal = new TagDal();
            if (tagdal.Exist(x => x.TagName == model.TagName)) { return "存在同名标签"; }

            return tagdal.Add(model) > 0 ? SUCCESS : "添加失败";
        }

        /// <summary>
        /// 更新标签
        /// </summary>
        /// <param name="updatemodel"></param>
        /// <returns></returns>
        public string UpdateTag(TagModel updatemodel)
        {
            if (updatemodel.TagID <= 0) { return "ID为空"; }

            var tagdal = new TagDal();
            var model = tagdal.GetFirst(x => x.TagID == updatemodel.TagID);
            if (model == null) { return "数据不存在"; }

            model.TagName = updatemodel.TagName;
            model.TagDesc = updatemodel.TagDesc;
            model.TagLink = updatemodel.TagLink;

            if (tagdal.Exist(x => x.TagName == model.TagName && x.TagID != model.TagID)) { return "存在同名标签"; }

            return tagdal.Update(model) > 0 ? SUCCESS : "添加失败";
        }

        public string DeleteTag(int id)
        {
            if (id <= 0) { return "ID为空"; }
            var tagdal = new TagDal();
            var model = tagdal.GetFirst(x => x.TagID == id);
            if (model == null) { return "数据不存在"; }
            return tagdal.Delete(model) > 0 ? SUCCESS : "删除失败";
        }

    }
}
