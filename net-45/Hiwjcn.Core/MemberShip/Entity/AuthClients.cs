using Hiwjcn.Core.Data;
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
    public class AuthClient : AuthClientBase, IMemberShipDBTable { }

    [Serializable]
    [Table("auth_scope")]
    public class AuthScope : AuthScopeBase, IMemberShipDBTable { }

    [Serializable]
    [Table("auth_token")]
    public class AuthToken : AuthTokenBase, IMemberShipDBTable { }

    [Serializable]
    [Table("auth_token_scope")]
    public class AuthTokenScope : AuthTokenScopeBase, IMemberShipDBTable { }

    [Serializable]
    [Table("auth_code")]
    public class AuthCode : AuthCodeBase, IMemberShipDBTable { }
}
