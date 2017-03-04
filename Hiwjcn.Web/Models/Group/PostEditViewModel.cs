using Model.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebLogic.Model.Tag;

namespace Hiwjcn.Web.Models.Group
{
    public class PostEditViewModel : ViewModelBase
    {
        public virtual PostModel Post { get; set; }
    }
}