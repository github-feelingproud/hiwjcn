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
        Task<(AuthCode code, string msg)> CreateCodeAsync(string client_uid, List<string> scopes, string user_uid);

        Task<(AuthToken token, string msg)> CreateTokenAsync(string client_id, string client_secret, string code_uid);

        Task<AuthToken> FindTokenAsync(string client_uid, string token_uid);

        Task<string> DeleteClientAsync(string client_uid, string user_uid);

        Task<PagerData<AuthClient>> GetMyAuthorizedClientsAsync(string user_id, string q, int page, int pagesize);
    }
}
