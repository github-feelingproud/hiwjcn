using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure;
using Hiwjcn.Core.Domain.Auth;

namespace Hiwjcn.Core.Infrastructure.Auth
{
    public interface IAuthScopeService : IServiceBase<AuthScope>
    {
        Task<string> AddScopeAsync(AuthScope scope);

        Task<string> DeleteScope(string scope_uid);

        Task<string> UpdateScope(AuthScope updatemodel);
    }
}
