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

namespace Lib.mvc.auth.api
{
    /// <summary>
    /// 基于web api远程调用的auth api实现
    /// </summary>
    [Obsolete("使用wcf的provider，不提供webapi的支持")]
    public class AuthApiFromWebApi : IAuthApi
    {
        public Task<_<TokenModel>> CreateAccessTokenAsync(string user_uid)
        {
            throw new NotImplementedException();
        }

        public Task<_<LoginUserInfo>> GetLoginUserInfoByTokenAsync(string access_token)
        {
            throw new NotImplementedException();
        }

        public Task<_<string>> RemoveCacheAsync(CacheBundle data)
        {
            throw new NotImplementedException();
        }

        public Task<_<LoginUserInfo>> ValidUserByOneTimeCodeAsync(string phone, string sms)
        {
            throw new NotImplementedException();
        }

        public Task<_<LoginUserInfo>> ValidUserByPasswordAsync(string username, string password)
        {
            throw new NotImplementedException();
        }
    }
}
