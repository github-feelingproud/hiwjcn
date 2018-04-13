using Hiwjcn.Core.Data;
using Lib.infrastructure.entity;
using Lib.infrastructure.entity.auth;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hiwjcn.Core.Domain.Auth
{
    [Serializable]
    [Table("auth_token")]
    public class AuthToken : AuthTokenBase, IMemberShipDBTable { }
}
