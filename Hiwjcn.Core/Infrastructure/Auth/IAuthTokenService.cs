using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure;
using Hiwjcn.Core.Domain.Auth;
using Lib.helper;

namespace Hiwjcn.Core.Infrastructure.Auth
{
    public interface IAuthTokenService : IServiceBase<AuthToken>
    {
        Task<string> CreateToken(string client_uid, string user_uid)

        Task<AuthToken> FindToken(string token_uid);

        Task<AuthToken> RefreshToken(string refresh_token_uid);

        Task<PagerData<AuthToken>> GetMyAuthorizedClients(string user_id, int page, int pagesize);
    }
}
