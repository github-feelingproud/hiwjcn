using Hiwjcn.Bll.Hotel;
using Hiwjcn.Core.Model.Hotel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hiwjcn.Web.Models.Hotel
{
    public class PriceTempEditViewModel : ViewModelBase
    {
        public virtual RoomTypeModel RoomType { get; set; }

        public virtual PriceTempModel PriceTemp { get; set; }
    }
}