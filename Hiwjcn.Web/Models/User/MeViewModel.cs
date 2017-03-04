using Model.Post;
using Model.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hiwjcn.Web.Models.User
{
    public class MeViewModel : ViewModelBase
    {
        public virtual bool IsMe { get; set; }

        public virtual UserModel User { get; set; }

        public virtual List<PostModel> PostList { get; set; }
    }
}