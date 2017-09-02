using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using Lib.helper;
using Lib.core;
using Lib.mvc.user;
using Lib.mvc;

namespace Lib.mvc.auth
{
    public interface IAuthApi
    {
        Task<_<TokenModel>> GetAccessToken(string client_id, string client_secret, string code, string grant_type);
    }
}
