using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebLogic.Model.Product;

namespace Hiwjcn.Web.Models.Shop
{
    public class ProductItemEditViewModel
    {
        public virtual int ProductID { get; set; }

        public virtual int ProductItemID { get; set; }

        public virtual ProductItemModel Item { get; set; }
    }
}