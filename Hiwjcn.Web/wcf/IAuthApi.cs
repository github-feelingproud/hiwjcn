using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Hiwjcn.Core.Domain.Auth;
using Lib.mvc;
using Lib.helper;
using Lib.extension;

namespace Hiwjcn.Web.wcf
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IAuthApi”。
    [ServiceContract]
    public interface IAuthApi
    {
        [OperationContract]
        Task<ResultMsg<AuthCodeWcf>> CreateCode(string client_uid, List<string> scopes, string user_uid);

        [OperationContract]
        Task<ResultMsg<AuthTokenWcf>> CreateToken(string client_uid, string client_secret, string code_uid);

        [OperationContract]
        Task<ResultMsg<AuthTokenWcf>> FindToken(string token_uid);

        [OperationContract]
        Task<PagerData<AuthClientWcf>> GetMyAuthorizedClients(string user_id, string q, int page, int pagesize);

        [OperationContract]
        Task<List<AuthScopeWcf>> GetScopesOrDefault(string[] scopes);
    }

    [Serializable]
    [DataContract]
    public class AuthTokenWcf
    {
        [DataMember]
        public string Token { get; set; }

        [DataMember]
        public string RefreshToken { get; set; }

        [DataMember]
        public DateTime Expire { get; set; }

        [DataMember]
        public List<string> Scopes { get; set; }

        [DataMember]
        public string UserUID { get; set; }

        public static implicit operator AuthTokenWcf(AuthToken token) =>
            new AuthTokenWcf()
            {
                Token = token.UID,
                RefreshToken = token.RefreshToken,
                Expire = token.ExpiryTime,
                Scopes = token.Scopes?.Select(x => x.Name).ToList(),
                UserUID = token.UserUID
            };
    }

    [Serializable]
    [DataContract]
    public class AuthCodeWcf
    {
        [DataMember]
        public string Code { get; set; }

        public static implicit operator AuthCodeWcf(AuthCode code) =>
            new AuthCodeWcf()
            {
                Code = code.UID
            };
    }

    [Serializable]
    [DataContract]
    public class AuthClientWcf
    {
        [DataMember]
        public virtual long IID { get; set; }
        [DataMember]
        public virtual string UID { get; set; }
        [DataMember]
        public virtual string ClientName { get; set; }
        [DataMember]
        public virtual string Description { get; set; }
        [DataMember]
        public virtual string ClientUrl { get; set; }
        [DataMember]
        public virtual string LogoUrl { get; set; }
        [DataMember]
        public virtual string UserUID { get; set; }
        [DataMember]
        public virtual string ClientSecretUID { get; set; }
        [DataMember]
        public virtual int IsRemove { get; set; }
        [DataMember]
        public virtual DateTime CreateTime { get; set; }
        [DataMember]
        public virtual DateTime? UpdateTime { get; set; }

        public static implicit operator AuthClientWcf(AuthClient client) =>
            new AuthClientWcf()
            {
                IID = client.IID,
                UID = client.UID,
                ClientName = client.ClientName,
                Description = client.Description,
                ClientUrl = client.ClientUrl,
                LogoUrl = client.LogoUrl,
                UserUID = client.UserUID,
                ClientSecretUID = client.ClientSecretUID,
                IsRemove = client.IsRemove,
                CreateTime = client.CreateTime,
                UpdateTime = client.UpdateTime
            };
    }

    [DataContract]
    public class AuthScopeWcf
    {
        [DataMember]
        public virtual int IID { get; set; }

        [DataMember]
        public virtual string UID { get; set; }

        [DataMember]
        public virtual string Name { get; set; }

        [DataMember]
        public virtual string DisplayName { get; set; }

        [DataMember]
        public virtual string Description { get; set; }

        [DataMember]
        public virtual int Important { get; set; }

        [DataMember]
        public virtual int Sort { get; set; }

        [DataMember]
        public virtual int IsDefault { get; set; }

        [DataMember]
        public virtual string ImageUrl { get; set; }

        [DataMember]
        public virtual string FontIcon { get; set; }

        [DataMember]
        public virtual DateTime CreateTime { get; set; }

        [DataMember]
        public virtual DateTime? UpdateTime { get; set; }

        public static implicit operator AuthScopeWcf(AuthScope scope) =>
            scope.ToJson().JsonToEntity<AuthScopeWcf>();
    }
}
