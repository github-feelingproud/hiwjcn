using Model.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hiwjcn.Web.Models.Category
{
    public class CategoryManageViewModel : ViewModelBase
    {
        public virtual string CategoryType { get; set; }

        public virtual bool HasNodes { get; set; }

        public virtual string Json { get; set; }

        public virtual List<string> TypesList { get; set; }

        public virtual List<CategoryModel> ErrList { get; set; }
    }
}