using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.mvc.auth
{
    public class TokenModel
    {
        public virtual string Token { get; set; }
        public virtual string RefreshToken { get; set; }
        public virtual DateTime Expire { get; set; }
    }
}
