using Hiwjcn.Bll.Sys;
using Hiwjcn.Core.Model.Sys;
using Lib.helper;
using System;
using System.Web.Mvc;

namespace Hiwjcn.Web.Controllers
{
    /// <summary>
    /// 评论
    /// </summary>
    public class CommentController : WebCore.MvcLib.Controller.UserController
    {
        private CommentBll _CommentBll { get; set; }
        public CommentController()
        {
            this._CommentBll = new CommentBll();
        }

        /// <summary>
        /// 评论框
        /// </summary>
        /// <param name="parentid"></param>
        /// <param name="threadid"></param>
        /// <returns></returns>
        public ActionResult Comment(int? parentid, string threadid)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                parentid = parentid ?? -1;

                if (!ValidateHelper.IsPlumpString(threadid))
                {
                    return Content("缺少参数：ThreadID");
                }

                ViewData["ThreadID"] = threadid;
                ViewData["ParentID"] = parentid.Value;

                return View();
            });
        }

        /// <summary>
        /// 发表评论
        /// </summary>
        /// <param name="threadid"></param>
        /// <param name="parentcommentid"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PostCommentAction(string threadid, int? parentcommentid, string comment)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var start = DateTime.Now.Date;
                var end = start.AddDays(1);
                if (_CommentBll.GetCommentCount(loginuser.IID, start, end) > 100)
                {
                    return GetJsonRes("你今天发布了太多评论，明天再来吧");
                }

                var model = new CommentModel()
                {
                    ThreadID = threadid,
                    ParentCommentID = parentcommentid ?? -1,
                    CommentContent = comment,
                    UserID = loginuser.IID,
                    UpdateTime = DateTime.Now
                };
                var res = _CommentBll.AddComment(model);
                return GetJsonRes(res);
            });
        }

        /// <summary>
        /// 删除评论
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteCommentAction(int? id)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                id = id ?? 0;
                
                var res = _CommentBll.DeleteComment(id.Value, loginuser.IID);
                return GetJsonRes(res);
            });
        }
    }
}