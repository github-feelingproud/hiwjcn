using Hiwjcn.Core.Model.Sys;
using Model.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hiwjcn.Web.Models.Group
{
    public class PostViewModel : ViewModelBase
    {
        public virtual PostModel Post { get; set; }

        public virtual int CommentsCount { get; set; }

        public virtual List<CommentModel> CommentsList { get; set; }

        public virtual string PagerHtml { get; set; }
    }
}