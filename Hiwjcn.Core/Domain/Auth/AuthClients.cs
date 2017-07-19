using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Model.User;

namespace Hiwjcn.Core.Domain.Auth
{
    /// <summary>
    /// auth客户端
    /// </summary>
    [Serializable]
    [Table("auth_client")]
    public class AuthClient : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(nameof(IID))]
        public virtual int IID { get; set; }

        [Index]
        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "客户端UID必填")]
        public virtual string UID { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "客户端名称必填")]
        public virtual string ClientName { get; set; }

        [Required]
        [Url(ErrorMessage = "客户端地址必须是URL")]
        [StringLength(1000, ErrorMessage = "客户端地址长度过长")]
        public virtual string ClientUrl { get; set; }

        [Required]
        [Url(ErrorMessage = "客户端图标必填")]
        [StringLength(1000, ErrorMessage = "客户端地址长度过长")]
        public virtual string LogoUrl { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "客户端用户UID必填")]
        public virtual string UserUID { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "客户端ClientSecretUID必填")]
        public virtual string ClientSecretUID { get; set; }

        public virtual int IsRemove { get; set; }

        public virtual DateTime CreateTime { get; set; }

        public virtual DateTime? UpdateTime { get; set; }
    }

    [Serializable]
    [Table("auth_client_useage")]
    public class AuthClientUseage : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int IID { get; set; }

        [Index]
        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "client useage UID必填")]
        public virtual string UID { get; set; }


        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "client uid 名称必填")]
        public virtual string ClientUID { get; set; }


        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "user uid 名称必填")]
        public virtual string UserUID { get; set; }

        public virtual DateTime CreateTime { get; set; }
    }

    [Serializable]
    [Table("auth_scope")]
    public class AuthScope : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int IID { get; set; }

        [Index]
        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "scope UID必填")]
        public virtual string UID { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "scope 名称必填")]
        public virtual string Name { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "scope 展示名称必填")]
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

        public virtual DateTime CreateTime { get; set; }

        public virtual DateTime? UpdateTime { get; set; }
    }

    [Serializable]
    [Table("auth_token")]
    public class AuthToken : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int IID { get; set; }

        [Index]
        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "token UID必填")]
        public virtual string UID { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "refresh token 必填")]
        public virtual string RefreshToken { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "客户端UID 必填")]
        public virtual string ClientUID { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "用户UID 必填")]
        public virtual string UserUID { get; set; }

        public virtual DateTime ExpiryTime { get; set; }

        public virtual DateTime CreateTime { get; set; }

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
    public class AuthTokenScope : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int IID { get; set; }

        [Index]
        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "auth token UID必填")]
        public virtual string UID { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "token UID 必填")]
        public virtual string TokenUID { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "scope UID 必填")]
        public virtual string ScopeUID { get; set; }

        public virtual DateTime CreateTime { get; set; }
    }

    [Serializable]
    [Table("auth_code")]
    public class AuthCode : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int IID { get; set; }

        [Index]
        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "token UID必填")]
        public virtual string UID { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "用户UID 必填")]
        public virtual string UserUID { get; set; }

        [Required(ErrorMessage = "客户端为空")]
        public virtual string ClientUID { get; set; }

        [Required(ErrorMessage = "scopes为空")]
        public virtual string ScopesJson { get; set; }

        public virtual DateTime CreateTime { get; set; }
    }
}
