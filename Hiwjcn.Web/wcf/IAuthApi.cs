using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Hiwjcn.Core.Domain.Auth;

namespace Hiwjcn.Web.wcf
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“IAuthApi”。
    [ServiceContract]
    public interface IAuthApi
    {
        [OperationContract]
        Task<AuthTokenWcf> CreateToken();
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

        public static implicit operator AuthTokenWcf(AuthToken token) =>
            new AuthTokenWcf()
            {
                Token = token.UID,
                RefreshToken = token.RefreshToken,
                Expire = token.ExpiryTime,
                Scopes = token.Scopes?.Select(x => x.Name).ToList()
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


}
