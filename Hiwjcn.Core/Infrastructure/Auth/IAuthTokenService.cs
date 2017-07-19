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
        Task<(AuthCode code, string msg)> CreateCode(string client_uid, List<string> scopes, string user_uid);

        Task<(AuthToken token, string msg)> CreateToken(string client_id, string client_secret, string code_uid);

        Task<AuthToken> FindToken(string token_uid);

        Task<string> DeleteToken(string client_uid, string user_uid);

        Task<PagerData<AuthToken>> GetMyAuthorizedClients(string user_id, int page, int pagesize);
    }
}
