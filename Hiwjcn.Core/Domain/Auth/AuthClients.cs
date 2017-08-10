using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Model.User;
using System.Runtime.Serialization;
using Model;

namespace Hiwjcn.Core.Domain.Auth
{
    /// <summary>
    /// auth客户端
    /// </summary>
    [Serializable]
    [Table("auth_client")]
    public class AuthClient : BaseEntity
    {
        [StringLength(20, MinimumLength = 1, ErrorMessage = "客户端名称必填")]
        [Required]
        [Index(IsUnique = true)]
        public virtual string ClientName { get; set; }

        [MaxLength(100, ErrorMessage = "描述过长")]
        public virtual string Description { get; set; }

        [StringLength(1000, ErrorMessage = "客户端地址长度过长")]
        [Url(ErrorMessage = "客户端地址必须是URL")]
        [Required]
        public virtual string ClientUrl { get; set; }

        [StringLength(1000, ErrorMessage = "客户端地址长度过长")]
        [Url(ErrorMessage = "客户端图标必填")]
        [Required]
        public virtual string LogoUrl { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "客户端用户UID必填")]
        [Required]
        [Index(IsUnique = false)]
        public virtual string UserUID { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "客户端ClientSecretUID必填")]
        [Required]
        [Index(IsUnique = true)]
        public virtual string ClientSecretUID { get; set; }

        public virtual int IsActive { get; set; }

        public virtual int IsOfficial { get; set; }
    }

    [Serializable]
    [Table("auth_client_checklog")]
    public class AuthClientCheckLog : BaseEntity
    {
        public virtual int CheckStatus { get; set; }

        public virtual string Msg { get; set; }
    }

    [Serializable]
    [Table("auth_client_useage")]
    public class AuthClientUseage : BaseEntity
    {
        [StringLength(100, MinimumLength = 20, ErrorMessage = "client uid 名称必填")]
        [Required]
        public virtual string ClientUID { get; set; }


        [StringLength(100, MinimumLength = 20, ErrorMessage = "user uid 名称必填")]
        [Required]
        public virtual string UserUID { get; set; }
    }

    [Serializable]
    [Table("auth_scope")]
    public class AuthScope : BaseEntity
    {
        [StringLength(100, ErrorMessage = "scope 名称必填")]
        [Required]
        [Index(IsUnique = true)]
        public virtual string Name { get; set; }

        [StringLength(100, ErrorMessage = "scope 展示名称必填")]
        [Required]
        public virtual string DisplayName { get; set; }

        [StringLength(100, ErrorMessage = "描述太长")]
        public virtual string Description { get; set; }

        public virtual int Important { get; set; }

        public virtual int Sort { get; set; }

        public virtual int IsDefault { get; set; }

        [StringLength(1000, ErrorMessage = "图片地址长度过长")]
        public virtual string ImageUrl { get; set; }

        [StringLength(30, ErrorMessage = "字体图标数据过长")]
        public virtual string FontIcon { get; set; }
    }

    [Serializable]
    [Table("auth_token")]
    public class AuthToken : BaseEntity
    {
        [StringLength(100, MinimumLength = 20, ErrorMessage = "refresh token 必填")]
        [Required]
        [Index(IsUnique = true)]
        public virtual string RefreshToken { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "客户端UID 必填")]
        [Required]
        [Index(IsUnique = false)]
        public virtual string ClientUID { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "用户UID 必填")]
        [Required]
        [Index(IsUnique = false)]
        public virtual string UserUID { get; set; }

        [DataType(DataType.Text)]
        public virtual string ScopesInfoJson { get; set; }

        [Index(IsUnique = false)]
        public virtual DateTime ExpiryTime { get; set; }

        public virtual DateTime? RefreshTime { get; set; }

        [NotMapped]
        public virtual List<AuthScope> Scopes { get; set; }

        [NotMapped]
        public virtual List<string> ScopeNames { get; set; }

        [NotMapped]
        public virtual List<string> ScopeUIDS { get; set; }

        [NotMapped]
        public virtual AuthClient Client { get; set; }
    }

    [Serializable]
    [Table("auth_token_scope")]
    public class AuthTokenScope : BaseEntity
    {
        [StringLength(100, MinimumLength = 20, ErrorMessage = "token UID 必填")]
        [Required]
        [Index(IsUnique = false)]
        public virtual string TokenUID { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "scope UID 必填")]
        [Required]
        [Index(IsUnique = false)]
        public virtual string ScopeUID { get; set; }
    }

    [Serializable]
    [Table("auth_code")]
    public class AuthCode : BaseEntity
    {
        [StringLength(100, MinimumLength = 20, ErrorMessage = "用户UID 必填")]
        [Required]
        [Index(IsUnique = false)]
        public virtual string UserUID { get; set; }

        [Required(ErrorMessage = "客户端为空")]
        [Index(IsUnique = false)]
        public virtual string ClientUID { get; set; }

        [Required(ErrorMessage = "scopes为空")]
        public virtual string ScopesJson { get; set; }
    }
}
