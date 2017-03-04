using Hiwjcn.Web.Models;
using Model.Post;
using Model.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebLogic.Model.Page;

namespace WebApp.Models.Group
{
    public class PostListViewModel : ViewModelBase
    {
        public virtual string Year { get; set; }

        public virtual string Month { get; set; }

        public virtual List<SectionModel> Sections { get; set; }

        public virtual List<LinkModel> Sliders { get; set; }

        public virtual List<PostModel> PostList { get; set; }

        public virtual List<string> SearchedTagList { get; set; }

        public virtual int PostCount { get; set; }

        public virtual string PagerHtml { get; set; }

        public virtual string PostJson { get; set; }
    }
}