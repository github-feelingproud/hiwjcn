using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Lib.mvc.user
{
    public interface IGetLoginUser
    {
        LoginUserInfo GetLoginUser(HttpContext context = null);
    }

    /// <summary>
    /// 获取user登录
    /// </summary>
    public class GetCommonLoginUser : IGetLoginUser
    {
        public LoginUserInfo GetLoginUser(HttpContext context = null)
        {
            return AccountHelper.User.GetLoginUser(context);
        }
    }

    /// <summary>
    /// 获取sso登录
    /// </summary>
    public class GetSSOLoginUser : IGetLoginUser
    {
        public LoginUserInfo GetLoginUser(HttpContext context = null)
        {
            return AccountHelper.SSO.GetLoginUser(context);
        }
    }

}
