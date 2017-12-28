using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Lib.mvc.auth
{
    [Serializable]
    public class UserRolePermission
    {
        public virtual List<string> Permissions { get; set; }
        public virtual List<string> Roles { get; set; }
    }

    [Serializable]
    [DataContract]
    public class TokenModel
    {
        [DataMember]
        public virtual string Token { get; set; }
        [DataMember]
        public virtual string RefreshToken { get; set; }
        [DataMember]
        public virtual DateTime Expire { get; set; }
    }

    [Serializable]
    [DataContract]
    public class CacheBundle
    {
        [DataMember]
        public virtual string[] UserUID { get; set; }

        [DataMember]
        public virtual string[] TokenUID { get; set; }

        [DataMember]
        public virtual string[] SSOUserUID { get; set; }

        [DataMember]
        public virtual string[] ClientUID { get; set; }

        [DataMember]
        public virtual string[] ScopeUID { get; set; }
    }
}
