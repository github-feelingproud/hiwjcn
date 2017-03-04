using Hiwjcn.Bll.Hotel;
using Hiwjcn.Core.Model.Hotel;
using Lib.core;
using Lib.helper;
using Lib.http;
using Lib.mvc;
using Lib.mvc.user;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebCore.MvcLib;
using WebCore.MvcLib.Controller;
namespace Hiwjcn.Framework.Controller
{
    public class HotelBaseController : UserController
    {
        protected readonly string HOTEL_COOKIE = "Hotel";
        [NonAction]
        protected void SetHotelCookie(string hotel_no)
        {
            CookieHelper.SetCookie(this.X.context, HOTEL_COOKIE, hotel_no);
        }
        [NonAction]
        protected ActionResult PrepareHotel(
            Func<LoginUserInfo, V_HotelManager, ActionResult> handler,
            params MemberType[] OnlyMemberList)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var hotel_id = CookieHelper.GetCookie(this.X.context, HOTEL_COOKIE);
                hotel_id = ConvertHelper.GetString(hotel_id).Trim();
                if (!ValidateHelper.IsPlumpString(hotel_id))
                {
                    return Redirect("/Hotel/Choose/");
                }
                var bll = new V_HotelManagerBll();
                var map = bll.GetManagerAuth(hotel_id, loginuser.IID);
                if (map == null) { return Content("没有权限"); }
                if (map.HotelDeleted > 0) { return Content("酒店被删除"); }
                if (ValidateHelper.IsPlumpList(OnlyMemberList))
                {
                    if (!OnlyMemberList.Select(x => (int)x).Contains(map.MemberType ?? int.MinValue))
                    {
                        return Content("无权访问");
                    }
                }
                return handler.Invoke(loginuser, map);
            });
        }
    }
}