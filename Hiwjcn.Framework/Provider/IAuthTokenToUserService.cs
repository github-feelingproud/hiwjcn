using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.ioc;
using Lib.mvc.user;
using Lib.mvc.auth;
using Lib.mvc.auth.validation;
using Lib.mvc;

namespace Hiwjcn.Core.Infrastructure.Auth
{
    public interface IAuthTokenToUserService : IAutoRegistered
    {
        Task<_<LoginUserInfo>> FindUserByTokenAsync(string access_token, string client_uid);
    }
}
