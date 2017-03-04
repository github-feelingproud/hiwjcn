using Hiwjcn.Core.Model.Product;
using Model.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebLogic.Model.Product;

namespace Hiwjcn.Web.Models.Shop
{
    public class GoodsListViewModel
    {
        public virtual ProductSearchModel Condition { get; set; }

        public virtual int SellerID { get; set; }

        public virtual List<ProductModel> ProductList { get; set; }

        public virtual string PagerHtml { get; set; }
    }
}