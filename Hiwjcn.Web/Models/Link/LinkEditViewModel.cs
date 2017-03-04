using Model.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hiwjcn.Web.Models.Link
{
    public class LinkEditViewModel : ViewModelBase
    {
        public virtual LinkModel Link { get; set; }

        public virtual string LinkType { get; set; }
    }
}