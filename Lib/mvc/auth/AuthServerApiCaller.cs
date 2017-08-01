using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.mvc.auth
{
    public class AuthServerApiCaller
    {
        private readonly AuthServerConfig _server;
        public AuthServerApiCaller(AuthServerConfig _server)
        {
            this._server = _server;
        }


    }
}
