using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Lib.mvc.user
{
    /// <summary>
    /// 获取登录URL
    /// </summary>
    public interface IGetLoginUrl
    {
        string GetUrl(string current_url = null);
    }

    /// <summary>
    /// 跳转本地登录
    /// </summary>
    public class GetLocalLoginUrl : IGetLoginUrl
    {
        public string GetUrl(string current_url = null)
        {
            var url = HttpContext.Current.Request.Url.ToString();
            return "/Account/Login";
        }
    }

    /// <summary>
    /// 跳转SSO登录
    /// </summary>
    public class GetSSOLoginUrl : IGetLoginUrl
    {
        public string GetUrl(string current_url = null)
        {
            var url = HttpContext.Current.Request.Url.ToString();
            return SSOClientHelper.BuildSSOLoginUrl(url);
        }
    }
}
