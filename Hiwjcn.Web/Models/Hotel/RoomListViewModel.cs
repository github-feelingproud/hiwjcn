using Hiwjcn.Bll.Hotel;
using Hiwjcn.Core.Model.Hotel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hiwjcn.Web.Models.Hotel
{
    public class RoomListViewModel : ViewModelBase
    {
        public virtual List<RoomModel> RoomList { get; set; }

        public virtual string Pager { get; set; }

        public virtual List<RoomCountGroupByStatus> GroupedStatusData { get; set; }

        public virtual List<RoomCountGroupByType> GroupedTypeData { get; set; }
    }
}