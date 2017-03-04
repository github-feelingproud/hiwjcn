using Dal.Post;
using Dal.User;
using Hiwjcn.Bll.Tag;
using Hiwjcn.Core.Model.Hotel;
using Hiwjcn.Dal.Tag;
using Lib.core;
using Lib.extension;
using Lib.helper;
using Model.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WebLogic.Model.Tag;

namespace Bll.Post
{
    /// <summary>
    /// 文章
    /// </summary>
    public class PostBll : ServiceBase<PostModel>
    {
        public static readonly string TagType = "post";
        public static readonly string TagMapType = "post";

        private PostDal _PostDal { get; set; }
        private UserDal _UserDal { get; set; }
        private TagMapDal _TagMapDal { get; set; }

        /// <summary>
        /// 初始化逻辑类，添加检查方法
        /// </summary>
        public PostBll()
        {
            this._PostDal = new PostDal();
            this._UserDal = new UserDal();
            this._TagMapDal = new TagMapDal();
        }

        public override string CheckModel(PostModel model)
        {
            if (model == null) { return "对象为空"; }
            if (!ValidateHelper.IsPlumpString(model.HotelNo)) { return "所属酒店为空"; }
            if (!ValidateHelper.IsPlumpString(model.Title))
            {
                return "标题未输入";
            }
            if (!ValidateHelper.IsPlumpString(model.Content))
            {
                return "请输入内容";
            }
            if (model.UpdateTime == null)
            {
                return "文章更新时间不能为空";
            }
            if (model.AuthorID <= 0)
            {
                return "缺少用户信息";
            }
            if (!ValidateHelper.IsPlumpString(model.PostGuid))
            {
                return "缺少唯一标识";
            }
            return string.Empty;
        }

        /// <summary>
        /// 删除文章，只有作者和管理员可以删除
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string DeletePost(int postID, int AuthorID, int? HotelMemberType)
        {
            var model = _PostDal.GetFirst(x => x.PostID == postID);
            if (model == null) { return "内容不存在"; }
            var canDelete = false;
            if (HotelMemberType == (int)MemberType.超级管理员)
            {
                canDelete = true;
            }
            if (model.AuthorID == AuthorID)
            {
                canDelete = true;
            }
            if (!canDelete)
            {
                return "无权删除";
            }

            return _PostDal.Delete(model) > 0 ? SUCCESS : "删除失败";
        }

        /// <summary>
        /// 更新文章
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="tags"></param>
        /// <param name="content"></param>
        /// <param name="preview"></param>
        /// <param name="alias"></param>
        /// <param name="relgroup"></param>
        /// <param name="post_type"></param>
        /// <param name="img"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public string UpdatePost(PostModel updateModel, int userid, List<string> tags = null)
        {
            var model = _PostDal.GetFirst(x => x.PostID == updateModel.PostID);
            if (model == null) { return "内容不存在"; }
            if (model.AuthorID != userid)
            {
                return "没有权限";
            }
            model.Title = updateModel.Title;
            model.Content = updateModel.Content;
            model.Preview = updateModel.Preview;
            model.UpdateTime = DateTime.Now;
            if (!ValidateHelper.IsPlumpString(model.PostGuid))
            {
                model.PostGuid = Guid.NewGuid().ToString();
            }

            //检查是否符合数据库约束
            string check = CheckModel(model);
            if (ValidateHelper.IsPlumpString(check)) { return check; }

            if (_PostDal.Update(model) <= 0) { return "修改失败"; }

            var res = new TagMapBll().SaveTagsMap(model.PostGuid, TagMapType, tags);

            return ValidateHelper.IsPlumpString(res) ? "文章保存成功，但是标签保存失败" : SUCCESS;
        }

        /// <summary>
        /// 添加文章
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ClearIfExist"></param>
        /// <returns></returns>
        public string AddPost(PostModel model, List<string> tags = null)
        {
            string check = CheckModel(model);
            if (ValidateHelper.IsPlumpString(check)) { return check; }

            if (_PostDal.Add(model) <= 0) { return "发布失败"; }

            var res = new TagMapBll().SaveTagsMap(model.PostGuid, TagMapType, tags);

            return ValidateHelper.IsPlumpString(res) ? "文章保存成功，但是标签保存失败" : SUCCESS;
        }
        public PostModel LoadEx(PostModel model)
        {
            var list = LoadExList(new List<PostModel>() { model });
            if (ValidateHelper.IsPlumpList(list)) { return list[0]; }
            return null;
        }
        public List<PostModel> LoadExList(List<PostModel> list)
        {
            if (!ValidateHelper.IsPlumpList(list) || list.Where(x => x == null).Count() > 0) { return list; }
            #region 读取文章的附加信息
            //作者
            var useridlist = list.Select(x => x.AuthorID).Distinct().ToList();
            var userlist = _UserDal.GetList(x => useridlist.Contains(x.UserID));
            if (ValidateHelper.IsPlumpList(userlist))
            {
                list.ForEach(x =>
                {
                    x.UserModel = userlist.Where(m => m.UserID == x.AuthorID).FirstOrDefault();
                });
            }
            //标签
            var tagkeylist = list.Select(x => x.PostGuid).Distinct().ToList();
            var tagmaplist = _TagMapDal.GetList(x => tagkeylist.Contains(x.MapKey) && x.MapType == TagMapType);
            if (ValidateHelper.IsPlumpList(tagmaplist))
            {
                list.ForEach(x =>
                {
                    x.PostTagsList = tagmaplist.Where(m => m.MapKey == x.PostGuid).Select(m => m.TagName).Distinct().ToList();
                });
            }
            #endregion
            return list;
        }

        /// <summary>
        /// 通过ID或者别名读取文章
        /// </summary>
        /// <param name="id"></param>
        /// <param name="alias"></param>
        /// <param name="loadMeta"></param>
        /// <param name="AddReadTimes"></param>
        /// <returns></returns>
        public PostModel GetPost(int id, string hotel_no, bool loadMeta = true, bool AddReadTimes = false)
        {
            if (id <= 0 || !ValidateHelper.IsPlumpString(hotel_no)) { return null; }
            var model = FindFirstEntity(x => x.PostID == id && x.HotelNo == hotel_no);
            if (model == null) { return null; }
            if (loadMeta)
            {
                model = LoadEx(model);
            }
            if (AddReadTimes)
            {
                _PostDal.AddReadCountByID(model.PostID);
            }
            return model;
        }

        public List<PostModel> GetStickyTopList(string hotel_no, bool loadMeta = true, int count = 5)
        {
            if (!ValidateHelper.IsPlumpString(hotel_no)) { return new List<PostModel>(); }
            var list = _PostDal.QueryList(
                where: x => x.HotelNo == hotel_no && x.StickyTop == "true",
                orderby: x => x.UpdateTime,
                count: count);
            if (ValidateHelper.IsPlumpList(list) && loadMeta)
            {
                list = LoadExList(list);
            }
            return list;
        }

        /// <summary>
        /// 查询列表
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="uid"></param>
        /// <param name="tag"></param>
        /// <param name="q"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="OnlyStickyTop"></param>
        /// <param name="OnlyNotStickTop"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="loadCount"></param>
        /// <param name="loadMeta"></param>
        /// <returns></returns>
        public PagerData<PostModel> GetPager(string hotel_no,
            int uid = 0, List<string> tag = null, List<string> q = null, List<int> notShowIDList = null,
            int page = 1, int pagesize = 15, bool loadMeta = true)
        {
            if (!ValidateHelper.IsPlumpString(hotel_no)) { return null; }

            tag = ConvertHelper.NotNullList(tag).Where(x => ValidateHelper.IsPlumpString(x)).Distinct().ToList();
            q = ConvertHelper.NotNullList(q).Where(x => ValidateHelper.IsPlumpString(x)).Distinct().ToList();
            notShowIDList = ConvertHelper.NotNullList(notShowIDList).Where(x => x > 0).Distinct().ToList();

            var model = new PagerData<PostModel>();

            #region 读取列表
            _PostDal.PrepareSession((session) =>
            {
                var query = session.Set<PostModel>().AsQueryable();
                query = query.Where(x => x.HotelNo == hotel_no);
                if (ValidateHelper.IsPlumpList(notShowIDList))
                {
                    query = query.Where(x => !notShowIDList.Contains(x.PostID));
                }
                //作者
                if (uid > 0)
                {
                    query = query.Where(x => x.AuthorID == uid);
                    model.UrlParams["user_id"] = uid.ToString();
                }
                //关键词
                if (ValidateHelper.IsPlumpList(q))
                {
                    Expression<Func<PostModel, bool>> express = x => false;
                    foreach (var keyword in q)
                    {
                        express = express.Or(x => x.Title.Contains(keyword));
                    }
                    query = query.Where(express);
                    model.UrlParams["q"] = string.Join(",", q);
                }
                //标签
                if (ValidateHelper.IsPlumpList(tag))
                {
                    //Expression<Func<TagMapModel, bool>> express = x => false;
                    //foreach (var t in tag)
                    //{
                    //    express = express.Or(x => x.TagName == t);
                    //}
                    //var tagmapquery = session.Query<TagMapModel>();
                    //tagmapquery = tagmapquery.Where(m => m.MapType == TagMapType).Where(express);

                    //query = query.Where(x => tagmapquery.Select(m => m.MapKey).Contains(x.PostGuid));
                    //model.UrlParams["tag"] = string.Join(",", tag);

                    var tagmapquery = session.Set<TagMapModel>().AsQueryable();
                    tagmapquery = tagmapquery.Where(x => x.MapType == TagMapType && tag.Contains(x.TagName));

                    query = query.Where(x => tagmapquery.Select(m => m.MapKey).Contains(x.PostGuid));
                    model.UrlParams["tag"] = string.Join(",", tag);
                }
                //时间筛选
                //查询总数
                //model.ItemCount = CriteriaTransformer.Clone(query)
                //.SetProjection(NHibernate.Criterion.Projections.RowCount()).UniqueResult<int>();
                model.ItemCount = query.Count();
                var range = PagerHelper.GetQueryRange(page, pagesize);
                model.DataList = query.OrderByDescending(x => x.UpdateTime).Skip(range[0]).Take(range[1]).ToList();
                return true;
            });
            #endregion

            #region 读取文章的附加信息
            if (loadMeta && ValidateHelper.IsPlumpList(model.DataList))
            {
                model.DataList = LoadExList(model.DataList);
            }
            #endregion

            return model;
        }

        /// <summary>
        /// 获取文章数量
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="user_id"></param>
        /// <returns></returns>
        public int GetPostCount(int user_id, DateTime start, DateTime end)
        {
            return _PostDal.GetCount(
                x => x.AuthorID == user_id && x.UpdateTime >= start && x.UpdateTime < end);
        }

    }
}
