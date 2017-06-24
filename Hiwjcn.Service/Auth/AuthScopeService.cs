using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Core.Infrastructure.Auth;
using Lib.infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using Lib.helper;
using Lib.core;

namespace Hiwjcn.Bll.Auth
{
    public class AuthScopeService : ServiceBase<AuthScope>, IAuthScopeService
    {
        private readonly IRepository<AuthScope> _AuthScopeRepository;
        public AuthScopeService(IRepository<AuthScope> _AuthScopeRepository)
        {
            this._AuthScopeRepository = _AuthScopeRepository;
        }

        public async Task<string> AddScopeAsync(AuthScope scope)
        {
            scope.UID = Com.GetUUID();
            scope.CreateTime = DateTime.Now;
            scope.UpdateTime = null;
            if (!this.CheckModel(scope, out var msg))
            {
                return msg;
            }
            var res = await this._AuthScopeRepository.AddAsync(scope);
            return res > 0 ? SUCCESS : "保存失败";
        }

        public override string CheckModel(AuthScope model)
        {
            return base.CheckModel(model);
        }

    }
}
