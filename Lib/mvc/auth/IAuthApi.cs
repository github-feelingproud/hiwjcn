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
        public AuthApiFromWcf(string url)
        {
            this.url = ConvertHelper.GetString(url).Trim();

            var start_ok = this.url.ToLower().StartsWith("http://") || this.url.ToLower().StartsWith("https://");
            if (!start_ok || !this.url.ToLower().EndsWith(".svc"))
            {
                throw new Exception("非有效的wcf服务地址");
            }
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
}
