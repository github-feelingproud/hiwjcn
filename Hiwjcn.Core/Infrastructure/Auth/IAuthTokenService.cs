using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure;
using Hiwjcn.Core.Domain.Auth;

namespace Hiwjcn.Core.Infrastructure.Auth
{
    public interface IAuthTokenService : IServiceBase<AuthToken>
    {
        Task<string> CreateToken(AuthToken token);

        Task<AuthToken> FindToken(string token_uid);

        Task<string> RefreshToken(string refresh_token_uid);
    }
}
