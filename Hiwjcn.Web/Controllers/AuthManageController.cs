using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lib.mvc;
using Lib.mvc.auth;
using Lib.mvc.user;
using Lib.helper;
using Lib.core;
using Lib.extension;
using Lib.net;
using System.Threading.Tasks;
using Hiwjcn.Core.Infrastructure.Auth;

namespace Hiwjcn.Web.Controllers
{
    public class AuthManageController : BaseController
    {
        public const string Permission = "auth.manage";

        public AuthManageController()
        {
            //
        }

        [PageAuth(Permission = Permission)]
        public async Task<ActionResult> Scopes()
        {
            throw new NotImplementedException();
        }

        [ApiAuth(Permission = Permission)]
        public async Task<ActionResult> SaveScopeAction()
        {
            throw new NotImplementedException();
        }

        [PageAuth(Permission = Permission)]
        public async Task<ActionResult> Clients()
        {
            throw new NotImplementedException();
        }
    }
}