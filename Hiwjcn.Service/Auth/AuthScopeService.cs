using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Core.Infrastructure.Auth;
using Lib.infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;

namespace Hiwjcn.Bll.Auth
{
    public class AuthScopeService : ServiceBase<AuthScope>, IAuthScopeService
    {
        private readonly IRepository<AuthScope> _AuthScopeRepository;
        public AuthScopeService(IRepository<AuthScope> _AuthScopeRepository)
        {
            this._AuthScopeRepository = _AuthScopeRepository;
        }

        public override string CheckModel(AuthScope model)
        {
            return base.CheckModel(model);
        }

    }
}
