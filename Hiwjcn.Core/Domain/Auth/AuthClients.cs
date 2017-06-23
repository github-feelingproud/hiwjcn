using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Hiwjcn.Core.Domain.Auth
{
    /// <summary>
    /// auth客户端
    /// </summary>
    [Table(nameof(AuthClient))]
    public class AuthClient : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(nameof(IID))]
        public virtual int IID { get; set; }

        [Index]
        [Required(ErrorMessage = "客户端UID必填")]
        public virtual string UID { get; set; }

        [Required(ErrorMessage = "客户端名称必填")]
        public virtual string ClientName { get; set; }

        [Required(ErrorMessage = "客户端URL必填")]
        public virtual string ClientUrl { get; set; }

        [Url(ErrorMessage = "客户端图标必填")]
        public virtual string LogoUrl { get; set; }

        [Required(ErrorMessage = "客户端用户UID必填")]
        public virtual string UserUID { get; set; }

        public virtual int IsRemove { get; set; }

        public virtual DateTime CreateTime { get; set; }

        public virtual DateTime? UpdateTime { get; set; }
    }

    public class AuthScope : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int IID { get; set; }

        [Index]
        [Required(ErrorMessage = "scope UID必填")]
        public virtual string UID { get; set; }

        [Required(ErrorMessage = "scope 名称必填")]
        public virtual string Name { get; set; }

        [Required(ErrorMessage = "scope 展示名称必填")]
        public virtual string DisplayName { get; set; }

        public virtual string Description { get; set; }

        public virtual int Important { get; set; }

        public virtual int Sort { get; set; }

        public virtual string ImageUrl { get; set; }

        public virtual string FontIcon { get; set; }

        public virtual DateTime CreateTime { get; set; }

        public virtual DateTime? UpdateTime { get; set; }
    }

    public class AuthToken : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int IID { get; set; }

        [Index]
        [Required(ErrorMessage = "token UID必填")]
        public virtual string UID { get; set; }

        [Required(ErrorMessage = "token 必填")]
        public virtual string Token { get; set; }

        [Required(ErrorMessage = "refresh token 必填")]
        public virtual string RefreshToken { get; set; }

        [Required(ErrorMessage = "客户端UID 必填")]
        public virtual string ClientUID { get; set; }

        public virtual DateTime ExpiryTime { get; set; }

        public virtual DateTime CreateTime { get; set; }
    }

    public class AuthTokenScope : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int IID { get; set; }

        [Index]
        [Required(ErrorMessage = "auth token UID必填")]
        public virtual string UID { get; set; }

        [Required(ErrorMessage = "token UID 必填")]
        public virtual string TokenUID { get; set; }

        [Required(ErrorMessage = "scope UID 必填")]
        public virtual string ScopeUID { get; set; }

        public virtual DateTime CreateTime { get; set; }
    }
}
