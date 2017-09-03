using Lib.mvc.auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc;
using Lib.mvc.user;

namespace Hiwjcn.Web.Service
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IAuthApiService”。
    [ServiceContract]
    public interface IAuthApiService
    {
        [OperationContract]
        Task<_<TokenModel>> GetAccessToken(string client_id, string client_secret, string code, string grant_type);

        [OperationContract]
        Task<_<LoginUserInfo>> GetLoginUserInfoByToken(string client_id, string access_token);

        [OperationContract]
        Task<_<string>> GetAuthCodeByOneTimeCode(string client_id, string scopeJson, string phone, string sms);

        [OperationContract]
        Task<_<string>> GetAuthCodeByPassword(string client_id, string scopeJson, string username, string password);
    }
}
