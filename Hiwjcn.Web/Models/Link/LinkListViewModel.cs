using Model.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hiwjcn.Web.Models.Link
{
    public class LinkListViewModel : ViewModelBase
    {
        public virtual string LinkType { get; set; }

        public virtual List<LinkModel> LinkList { get; set; }
    }
}