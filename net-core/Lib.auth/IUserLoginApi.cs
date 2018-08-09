using System;
using System.Threading.Tasks;

namespace Lib.auth
{
    public interface IUserLoginApi
    {
        /// <summary>
        /// 用验证码登录换取code
        /// </summary>
        Task<_<LoginUserInfo>> ValidUserByOneTimeCodeAsync(string phone, string sms);

        /// <summary>
        /// 用密码登录换取code
        /// </summary>
        Task<_<LoginUserInfo>> ValidUserByPasswordAsync(string username, string password);

        /// <summary>
        /// 通过用户uid获取用户信息
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        Task<LoginUserInfo> GetLoginUserInfoByUserUID(string uid);
    }
}
