using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure;
using Hiwjcn.Core.Infrastructure.Auth;
using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Core.Data.Auth;
using Lib.data;
using Lib.extension;

namespace Hiwjcn.Bll.Auth
{
    public class AuthTokenScopeService : ServiceBase<AuthTokenScope>, IAuthTokenScopeService
    {
        private IRepository<AuthTokenScope> _AuthTokenScopeRepository;
        public AuthTokenScopeService(IRepository<AuthTokenScope> _AuthTokenScopeRepository)
        {
            this._AuthTokenScopeRepository = _AuthTokenScopeRepository;
        }

        public override string CheckModel(AuthTokenScope model)
        {
            return base.CheckModel(model);
        }

    }
}
