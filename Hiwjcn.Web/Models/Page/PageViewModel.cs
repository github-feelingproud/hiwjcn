using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebLogic.Model.Page;

namespace Hiwjcn.Web.Models.Page
{
    public class PageViewModel : ViewModelBase
    {
        public virtual string CurrentPageName { get; set; }

        public virtual SectionModel Page { get; set; }

        public virtual List<SectionModel> PageList { get; set; }
    }
}