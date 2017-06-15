using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiwjcn.Core.Domain.Auth
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthClients
    {
        public virtual int IID { get; set; }
        public virtual string UID { get; set; }
        public virtual int Enabled { get; set; }
        public virtual string ClientName { get; set; }
        public virtual string ClientUrl { get; set; }
        public virtual string LogoUrl { get; set; }
        public virtual int RequireConsent { get; set; }
        public virtual int AllowRememberConsent { get; set; }
        public virtual int Flow { get; set; }
    }
}
