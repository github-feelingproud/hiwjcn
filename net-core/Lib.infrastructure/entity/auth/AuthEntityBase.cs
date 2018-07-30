using Lib.extension;
using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;

namespace Lib.infrastructure.entity.auth
{
    public static class TokenConfig
    {
        public static readonly int TokenExpireDays =
            (ConfigurationManager.AppSettings["AuthExpireDays"] ?? "30").ToInt(30);
    }

    [Serializable]
    public class AuthTokenBase : BaseEntity
    {
        [StringLength(100, MinimumLength = 20, ErrorMessage = "refresh token 必填")]
        [Required]
        public virtual string RefreshToken { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "用户UID 必填")]
        [Required]
        public virtual string UserUID { get; set; }

        public virtual DateTime ExpiryTime { get; set; }

        public virtual DateTime? RefreshTime { get; set; }
    }

    /// <summary>
    /// open id
    /// </summary>
    [Serializable]
    public class OpenIdBase : BaseEntity
    {
        [Required]
        public virtual string ClientUID { get; set; }

        [Required]
        public virtual string UserUID { get; set; }
    }

    /// <summary>
    /// 外部登录，账号关联
    /// </summary>
    [Serializable]
    public class ExternalLoginMapBase : BaseEntity
    {
        [Required]
        public virtual string UserUID { get; set; }

        [Required]
        public virtual string AccessToken { get; set; }

        [Required]
        public virtual DateTime AccessTokenExpireAt { get; set; }

        [Required]
        public virtual string OpenID { get; set; }

        [Required]
        public virtual string RefreshToken { get; set; }

        [Required]
        public virtual string ProviderKey { get; set; }

        [Required]
        public virtual string ProviderName { get; set; }
    }
}
