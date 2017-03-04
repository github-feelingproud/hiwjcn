using Model.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebLogic.Model.Product;

namespace Hiwjcn.Web.Models.Shop
{
    public class ProductItemListViewModel
    {
        public virtual ProductModel Good { get; set; }

        public virtual List<ProductItemModel> ItemList { get; set; }
    }
}