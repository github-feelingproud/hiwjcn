using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Lib.extension;
using Lib.io;
using Lib.ioc;
using Lib.mvc.user;
using System.Reflection;
using System.Web.SessionState;

namespace Lib.mvc.auth
{
    public class AuthContext
    {
        private readonly HttpContext _context;
        private readonly ITokenProvider _tokenProvider;

        public AuthContext(HttpContext context, ITokenProvider tokenProvider)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }
    }
}
