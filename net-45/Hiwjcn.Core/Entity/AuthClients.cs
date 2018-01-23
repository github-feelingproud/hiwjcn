using Lib.infrastructure.entity;
using Lib.infrastructure.entity.auth;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hiwjcn.Core.Domain.Auth
{
    /// <summary>
    /// auth客户端
    /// </summary>
    [Serializable]
    [Table("auth_client")]
    public class AuthClient : AuthClientBase { }
    
    [Serializable]
    [Table("auth_scope")]
    public class AuthScope : AuthScopeBase { }

    [Serializable]
    [Table("auth_token")]
    public class AuthToken : AuthTokenBase { }

    [Serializable]
    [Table("auth_token_scope")]
    public class AuthTokenScope : AuthTokenScopeBase { }

    [Serializable]
    [Table("auth_code")]
    public class AuthCode : AuthCodeBase { }
}
