using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using Lib.helper;
using Lib.core;
using Lib.mvc.user;
using Lib.mvc;
using Lib.ioc;

namespace Lib.mvc.auth
{
    public interface IAuthApi : IAutoRegistered
    {
        Task<_<TokenModel>> GetAccessTokenAsync(string client_id, string client_secret, string code, string grant_type);

        Task<_<LoginUserInfo>> GetLoginUserInfoByTokenAsync(string client_id, string access_token);

        Task<_<string>> GetAuthCodeByOneTimeCodeAsync(string client_id, string scopeJson, string phone, string sms);

        Task<_<string>> GetAuthCodeByPasswordAsync(string client_id, string scopeJson, string username, string password);
    }
}
