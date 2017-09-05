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

namespace Lib.mvc.auth
{
    /// <summary>
    /// 对外提供的服务（webapi，wcf）
    /// </summary>
    public interface IAuthApi
    {
        Task<_<TokenModel>> GetAccessTokenAsync(string client_id, string client_secret, string code, string grant_type);

        Task<_<LoginUserInfo>> GetLoginUserInfoByTokenAsync(string client_id, string access_token);

        Task<_<string>> GetAuthCodeByOneTimeCodeAsync(string client_id, string scopeJson, string phone, string sms);

        Task<_<string>> GetAuthCodeByPasswordAsync(string client_id, string scopeJson, string username, string password);
    }

    /// <summary>
    /// wcf服务的约定，把这个发布到nuget用于调用
    /// 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IAuthApiService”。
    /// </summary>
    [ServiceContract]
    public interface IAuthApiWcfServiceContract
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

    /// <summary>
    /// 基于wcf远程调用的auth api实现
    /// </summary>
    public class AuthApiFromWcf : IAuthApi
    {
        private readonly string url;
        public AuthApiFromWcf(AuthServerConfig _server)
        {
            this.url = _server.WcfUrl;
        }

        public async Task<_<TokenModel>> GetAccessTokenAsync(string client_id, string client_secret, string code, string grant_type)
        {
            using (var client = new ServiceClient<IAuthApiWcfServiceContract>(this.url))
            {
                return await client.Instance.GetAccessToken(client_id, client_secret, code, grant_type);
            }
        }

        public async Task<_<string>> GetAuthCodeByOneTimeCodeAsync(string client_id, string scopeJson, string phone, string sms)
        {
            using (var client = new ServiceClient<IAuthApiWcfServiceContract>(this.url))
            {
                return await client.Instance.GetAuthCodeByOneTimeCode(client_id, scopeJson, phone, sms);
            }
        }

        public async Task<_<string>> GetAuthCodeByPasswordAsync(string client_id, string scopeJson, string username, string password)
        {
            using (var client = new ServiceClient<IAuthApiWcfServiceContract>(this.url))
            {
                return await client.Instance.GetAuthCodeByPassword(client_id, scopeJson, username, password);
            }
        }

        public async Task<_<LoginUserInfo>> GetLoginUserInfoByTokenAsync(string client_id, string access_token)
        {
            using (var client = new ServiceClient<IAuthApiWcfServiceContract>(this.url))
            {
                return await client.Instance.GetLoginUserInfoByToken(client_id, access_token);
            }
        }
    }

    /// <summary>
    /// 基于web api远程调用的auth api实现
    /// </summary>
    public class AuthApiFromWebApi : IAuthApi
    {
        private static readonly HttpClient client = HttpClientManager.Instance.DefaultClient;

        private readonly AuthServerConfig _server;
        public AuthApiFromWebApi(AuthServerConfig _server)
        {
            this._server = _server;
        }

        public async Task<_<TokenModel>> GetAccessTokenAsync(string client_id, string client_secret, string code, string grant_type)
        {
            var data = new _<TokenModel>();
            var response = await client.PostAsJsonAsync(this._server.CreateToken(), new
            {
                client_id = client_id,
                client_secret = client_secret,
                code = code,
                grant_type = grant_type
            });
            using (response)
            {
                var json = await response.Content.ReadAsStringAsync();
                var token = json.JsonToEntity<_<TokenModel>>();
                if (!token.success)
                {
                    data.SetErrorMsg(token.msg);
                    return data;
                }
                data.SetSuccessData(token.data);
            }
            return data;
        }

        public async Task<_<string>> GetAuthCodeByOneTimeCodeAsync(string client_id, string scopeJson, string phone, string sms)
        {
            var data = new _<string>();
            var response = await client.PostAsJsonAsync(this._server.CreateCodeByOneTimeCode(), new
            {
                client_id = client_id,
                scope = scopeJson,
                phone = phone,
                sms = sms
            });
            using (response)
            {
                var json = await response.Content.ReadAsStringAsync();
                var code = json.JsonToEntity<_<string>>();
                if (!code.success || !ValidateHelper.IsPlumpString(code.data))
                {
                    data.SetErrorMsg(code.msg);
                    return data;
                }
                data.SetSuccessData(code.data);
            }
            return data;
        }

        public async Task<_<string>> GetAuthCodeByPasswordAsync(string client_id, string scopeJson, string username, string password)
        {
            var data = new _<string>();
            var response = await client.PostAsJsonAsync(this._server.CreateAuthCodeByPassword(), new
            {
                client_id = client_id,
                scope = scopeJson,
                username = username,
                password = password
            });
            using (response)
            {
                var json = await response.Content.ReadAsStringAsync();
                var code = json.JsonToEntity<_<string>>();
                if (!code.success || !ValidateHelper.IsPlumpString(code.data))
                {
                    data.SetErrorMsg(code.msg);
                    return data;
                }
                data.SetSuccessData(code.data);
            }
            return data;
        }

        public async Task<_<LoginUserInfo>> GetLoginUserInfoByTokenAsync(string client_id, string access_token)
        {
            var data = new _<LoginUserInfo>();
            var response = await client.PostAsJsonAsync(this._server.CheckToken(), new
            {
                client_id = client_id,
                access_token = access_token
            });
            using (response)
            {
                var json = await response.Content.ReadAsStringAsync();
                var loginuser = json.JsonToEntity<_<LoginUserInfo>>();
                if (!loginuser.success)
                {
                    data.SetErrorMsg(loginuser.msg);
                    return data;
                }
                data.SetSuccessData(loginuser.data);
            }
            return data;
        }
    }
}
