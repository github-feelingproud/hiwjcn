using Hiwjcn.Web.Models;
using Model.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models.Page
{
    public class HomeModel : ViewModelBase
    {
        public virtual List<LinkModel> HomeLinksList { get; set; }
    }
}