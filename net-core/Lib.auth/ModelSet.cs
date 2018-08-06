using System;
using System.Runtime.Serialization;

namespace Lib.mvc.auth
{
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
    }
}
