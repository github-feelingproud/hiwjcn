using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;

namespace Hiwjcn.Core.Domain.Auth
{
    public enum TokenTypeEnum : byte
    {
        AuthorizationCode = 1,
        TokenHandle = 2,
        RefreshToken = 3
    }

    /// <summary>
    /// 
    /// </summary>
    public class AuthClients : IDBTable
    {
        public virtual int IID { get; set; }
        public virtual string UID { get; set; }
        public virtual string ClientName { get; set; }
        public virtual string ClientUrl { get; set; }
        public virtual string LogoUrl { get; set; }

        public virtual string UserUID { get; set; }

        public virtual int IsRemove { get; set; }

        public virtual DateTime CreateTime { get; set; }

        public virtual DateTime? UpdateTime { get; set; }
    }

    public class AuthScope : IDBTable
    {
        public virtual int IID { get; set; }
        public virtual string UID { get; set; }
        public virtual string Name { get; set; }
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
        public virtual int IID { get; set; }
        public virtual string UID { get; set; }
        public virtual string Token { get; set; }
        public virtual string RefreshToken { get; set; }
        public virtual string ClientUID { get; set; }
        public virtual DateTime ExpiryTime { get; set; }
        public virtual DateTime CreateTime { get; set; }
    }

    public class AuthTokenScope : IDBTable
    {
        public virtual int IID { get; set; }
        public virtual string UID { get; set; }
        public virtual string TokenUID { get; set; }
        public virtual string ScopeUID { get; set; }
        public virtual DateTime CreateTime { get; set; }
    }
}
