using Bll;
using Hiwjcn.Core.Model.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;
using Hiwjcn.Dal.Sys;
using Bll.User;
using Lib.helper;
using Autofac;
using Hiwjcn.Core.Infrastructure.User;
using Lib.ioc;
using Lib.infrastructure;
using Lib.extension;

namespace Hiwjcn.Bll.Sys
{
    public class CommentBll : ServiceBase<CommentModel>
    {
        public CommentBll()
        {
            //
        }

        public override string CheckModel(CommentModel model)
        {
            if (model == null) { return "评论对象为空"; }

            if (!ValidateHelper.IsPlumpString(model.ThreadID))
            {
                return "主题ID为空";
            }

            if (model.UserID <= 0)
            {
                return "评论人为空";
            }

            if (!ValidateHelper.IsPlumpString(model.CommentContent))
            {
                return "评论内容为空";
            }

            if (model.CommentContent.Length > 1000)
            {
                return "超过允许的评论字符数";
            }

            if (model.UpdateTime == null)
            {
                model.UpdateTime = DateTime.Now;
            }

            return string.Empty;
        }

        public CommentModel GetCommentByID(int id)
        {
            var list = GetCommentsByIDS(id);
            if (ValidateHelper.IsPlumpList(list))
            {
                return list[0];
            }
            return null;
        }

        /// <summary>
        /// 获取多个评论对象
        /// </summary>
        /// <param name="idlist"></param>
        /// <returns></returns>
        public List<CommentModel> GetCommentsByIDS(params int[] idlist)
        {
            if (!ValidateHelper.IsPlumpList(idlist)) { return null; }
            string key = Com.GetCacheKey("commentbll.GetCommentsByIDS", string.Join(",", idlist));
            return Cache(key, () =>
            {
                var _commentDal = new CommentDal();
                return _commentDal.GetList(x => idlist.Contains(x.CommentID));
            });
        }

        /// <summary>
        /// 获取评论分页
        /// </summary>
        /// <param name="threadID"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public PagerData<CommentModel> GetComments(string threadID, int page, int pagesize)
        {
            if (!ValidateHelper.IsPlumpString(threadID)) { return null; }

            string key = Com.GetCacheKey("commentbll.getcomments", threadID, page.ToString(), pagesize.ToString());

            return Cache(key, () =>
            {
                var data = new PagerData<CommentModel>();
                var _commentDal = new CommentDal();
                _commentDal.PrepareIQueryable((query) =>
                {
                    query = query.Where(x => x.ThreadID == threadID);
                    data.ItemCount = query.Count();
                    data.DataList = query.OrderByDescending(x => x.CommentID).QueryPage(page, pagesize).ToList();
                    return true;
                });
                if (ValidateHelper.IsPlumpList(data.DataList))
                {
                    var useridlist = data.DataList.Select(x => x.UserID).Distinct().Where(x => x > 0).ToArray();
                    if (ValidateHelper.IsPlumpList(useridlist))
                    {
                        var userbll = AppContext.GetObject<IUserService>();
                        var userlist = userbll.GetUserByIDS(useridlist);
                        if (ValidateHelper.IsPlumpList(userlist))
                        {
                            data.DataList.ForEach(x =>
                            {
                                x.UserModel = userlist.Where(m => m.UserID == x.UserID).FirstOrDefault();
                            });
                        }
                    }

                    var parentidlist = data.DataList.Select(x => x.ParentCommentID).Distinct().Where(x => x > 0).ToArray();
                    if (ValidateHelper.IsPlumpList(parentidlist))
                    {
                        var commentslist = GetCommentsByIDS(parentidlist);
                        if (ValidateHelper.IsPlumpList(commentslist))
                        {
                            data.DataList.ForEach(x =>
                            {
                                x.ParentComment = commentslist.Where(m => m.CommentID == x.ParentCommentID).FirstOrDefault();
                            });
                        }
                    }
                }
                return data;
            });
        }

        /// <summary>
        /// 添加评论
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string AddComment(CommentModel model)
        {
            string check = CheckModel(model);
            if (ValidateHelper.IsPlumpString(check))
            {
                return check;
            }
            var _commentDal = new CommentDal();
            var count = _commentDal.Add(model);
            return count > 0 ? SUCCESS : "评论失败";
        }

        /// <summary>
        /// 删除评论
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public string DeleteComment(int id, int userid)
        {
            if (id <= 0 || userid <= 0) { return "参数错误"; }
            var _commentDal = new CommentDal();
            var model = _commentDal.GetFirst(x => x.CommentID == id && x.UserID == userid);
            if (model == null) { return "评论不存在"; }
            return _commentDal.Delete(model) > 0 ? SUCCESS : "删除失败";
        }

        /// <summary>
        /// 获取评论数
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public int GetCommentCount(int userid, DateTime? start, DateTime? end)
        {
            string key = Com.GetCacheKey("commentbll.getcommentcount",
                userid.ToString(), ConvertHelper.GetString(start), ConvertHelper.GetString(end));
            return Cache(key, () =>
            {
                var _commentDal = new CommentDal();
                var count = 0;
                _commentDal.PrepareIQueryable((query) =>
                {
                    if (userid > 0)
                    {
                        query = query.Where(x => x.UserID == userid);
                    }
                    if (start != null)
                    {
                        query = query.Where(x => x.UpdateTime >= start.Value);
                    }
                    if (end != null)
                    {
                        query = query.Where(x => x.UpdateTime < end.Value);
                    }
                    count = query.Count();
                    return true;
                });
                return count;
            });
        }

    }
}
