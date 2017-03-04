using Bll.Category;
using Bll.Post;
using Bll.Sys;
using Hiwjcn.Bll.Sys;
using Hiwjcn.Bll.Tag;
using Hiwjcn.Framework.Controller;
using Hiwjcn.Web.Models.Group;
using Lib.helper;
using Lib.core;
using Lib.model;
using Model.Category;
using Model.Post;
using Model.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebApp.Models.Group;
using WebCore.MvcLib;
using WebCore.MvcLib.Controller;
using WebLogic.Bll.Page;
using WebLogic.Bll.User;
using WebLogic.Model.Page;
using WebLogic.Model.User;
using Hiwjcn.Framework;
using Lib.mvc;

namespace Hiwjcn.Web.Controllers
{
    [ValidateInput(false)]
    public class GroupController : HotelBaseController
    {
        #region 依赖注入
        private PostBll _postbll { get; set; }
        #endregion

        public GroupController(PostBll postbll)
        {
            this._postbll = postbll;
        }

        /// <summary>
        /// 文章列表
        /// </summary>
        /// <returns></returns>
        public ActionResult PostList(int? uid, string tag, string q, int? page)
        {
            return PrepareHotel((loginuser, map) =>
            {
                var pagesize = 16;
                page = CheckPage(page);
                uid = uid ?? 0;

                var model = new PostListViewModel() { LoginUser = loginuser };
                model.PostList = new List<PostModel>();

                //置顶的数据
                var stickylist = _postbll.GetStickyTopList(map.HotelNo, count: 5);
                if (ValidateHelper.IsPlumpList(stickylist))
                {
                    model.PostList.AddRange(stickylist);
                }
                //不置顶的数据
                //搜索的标签
                model.SearchedTagList = ConvertHelper.GetString(tag).Split(',').Where(x => ValidateHelper.IsPlumpString(x)).ToList();
                //搜索的标签+用户关注的标签
                var taglist = new List<string>();
                taglist.AddRange(model.SearchedTagList);
                //搜索关键词
                var querylist = ConvertHelper.GetString(q).Split(',').Where(x => ValidateHelper.IsPlumpString(x)).ToList();

                var datalist = _postbll.GetPager(map.HotelNo,
                    uid: uid.Value, notShowIDList: stickylist.Select(x => x.PostID).ToList(),
                    tag: taglist, q: querylist, page: page.Value, pagesize: pagesize);

                //合并置顶和不置顶
                if (datalist != null)
                {
                    if (ValidateHelper.IsPlumpList(datalist.DataList))
                    {
                        model.PostList.AddRange(datalist.DataList);
                    }

                    model.PostCount = datalist.ItemCount;
                    model.PagerHtml = datalist.GetPagerHtml("/group/postlist/?", "page", page.Value, pagesize);
                }

                ViewData["model"] = model;

                return View();
            });
        }

        /// <summary>
        /// 文章阅读界面
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("Group/Post/{id}/")]
        public ActionResult Post(int? id, int? page)
        {
            return PrepareHotel((loginuser, map) =>
            {
                id = id ?? 0;
                if (id <= 0) { return Http404(); }
                page = CheckPage(page);
                var PAGESIZE = 16;

                var model = new PostViewModel() { LoginUser = loginuser };
                _postbll.UseCache = true;

                #region 加载文章
                var post = _postbll.GetPost(id.Value, map.HotelNo, true, true);
                //如果不存在显示404页面
                if (post == null) { return Http404(); }
                if (post.UserModel == null) { post.UserModel = new UserModel(); }
                model.Post = post;
                #endregion

                #region 加载评论
                var _commentBll = new CommentBll();
                var threadid = "post:" + post.PostID;
                var commentData = _commentBll.GetComments(threadid, page.Value, PAGESIZE);
                if (commentData != null)
                {
                    model.CommentsCount = commentData.ItemCount;
                    model.CommentsList = commentData.DataList;
                    model.PagerHtml = commentData.GetPagerHtml(
                        "/group/post/" + post.PostID + "/",
                        "page", page.Value, PAGESIZE);
                }
                #endregion

                ViewData["model"] = model;

                return View();
            });
        }

        /// <summary>
        /// 文章编辑页面
        /// </summary>
        /// <returns></returns>
        //[UserAuthorize(FunctionsEnum.发帖)]
        public ActionResult PostEdit(int? id)
        {
            return PrepareHotel((loginuser, map) =>
            {
                //读取文章
                id = id ?? 0;
                var post = _postbll.GetPost(id.Value, map.HotelNo, true, false);
                //判断权限
                if (post != null)
                {
                    //不是本人无权访问
                    if (!(loginuser.IID == post.AuthorID))
                    {
                        return Http403();
                    }
                }

                var model = new PostEditViewModel();
                model.Post = post;
                model.LoginUser = loginuser;

                ViewData["model"] = model;
                return View();
            });
        }

        /// <summary>
        /// 添加或者更新文章
        /// </summary>
        /// <param name="post_id"></param>
        /// <param name="post_title"></param>
        /// <param name="post_tags"></param>
        /// <param name="post_preview"></param>
        /// <param name="post_content"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidCSRF(nameof(SavePostAction))]
        [RequestLog]
        public ActionResult SavePostAction(int? post_id,
            string post_title, string post_tags,
            string post_preview, string post_content)
        {
            return PrepareHotel((loginuser, map) =>
            {
                post_id = post_id ?? 0;
                //切割标签
                var taglist = ConvertHelper.GetString(post_tags).Trim()
                    .Split(',').Where(x => ValidateHelper.IsPlumpString(x) && x.Length < 20).ToList();
                if (taglist.Count() > 3)
                {
                    return GetJsonRes("最多只能选择3个标签");
                }

                var model = new PostModel();
                model.PostID = post_id.Value;
                model.Title = post_title;

                model.Preview = post_preview;
                model.Content = post_content;

                model.UpdateTime = DateTime.Now;

                var res = string.Empty;

                if (post_id > 0)
                {
                    //update
                    res = _postbll.UpdatePost(model, loginuser.IID, taglist);
                }
                else
                {
                    //检查用户今天的发帖量
                    var start = DateTime.Now.Date;
                    var end = start.AddDays(1);
                    int count = _postbll.GetPostCount(loginuser.IID, start, end);
                    if (count > 100)
                    {
                        return GetJsonRes("您当天发帖量已达上限");
                    }

                    //add
                    model.HotelNo = map.HotelNo;
                    model.StickyTop = "false";
                    model.AuthorID = loginuser.IID;
                    model.CommentCount = model.ReadCount = 0;
                    model.PostGuid = Guid.NewGuid().ToString();
                    res = _postbll.AddPost(model, taglist);
                }

                return GetJsonRes(res);
            });
        }

        /// <summary>
        /// 删除文章
        /// </summary>
        /// <param name="post_id"></param>
        /// <returns></returns>
        [HttpPost]
        [RequestLog]
        public ActionResult DeletePostAction(int? post_id)
        {
            return PrepareHotel((loginuser, map) =>
            {
                post_id = post_id ?? 0;
                if (post_id <= 0)
                {
                    return GetJsonRes("参数错误");
                }
                //只有文章作者和酒店管理员可以删除
                var res = _postbll.DeletePost(post_id.Value, loginuser.IID, map.MemberType);

                return GetJsonRes(res);
            });
        }
    }
}
