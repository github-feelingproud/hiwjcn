using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using Lib.helper;
using Lib.core;
using Lib.mvc.user;
using Lib.rpc;
using Lib.mvc;
using Lib.ioc;
using System.ServiceModel;
using System.Net.Http;
using Lib.net;
using System.Runtime.Serialization;

namespace Lib.mvc.auth
{
    /// <summary>
    /// 对外提供的服务（webapi，wcf）
    /// </summary>
    public interface IAuthApi
    {
        /// <summary>
        /// 用code换token
        /// </summary>
        Task<_<TokenModel>> CreateAccessTokenAsync(string user_uid);

        /// <summary>
        /// 用token换取登录信息
        /// </summary>
        Task<_<LoginUserInfo>> GetLoginUserInfoByTokenAsync(string access_token);

        /// <summary>
        /// 用验证码登录换取code
        /// </summary>
        Task<_<LoginUserInfo>> ValidUserByOneTimeCodeAsync(string phone, string sms);

        /// <summary>
        /// 用密码登录换取code
        /// </summary>
        Task<_<LoginUserInfo>> ValidUserByPasswordAsync(string username, string password);

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<_<string>> RemoveCacheAsync(CacheBundle data);
    }
}
