using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Lib.ioc;
using Lib.helper;
using Lib.core;
using Lib.extension;
using Lib.mvc.user;

namespace Lib.mvc.auth.validation
{
    public interface ITokenValidationProvider
    {
        LoginUserInfo FindUser(HttpContext context);

        Task<LoginUserInfo> FindUserAsync(HttpContext context);
    }

    public class CookieTokenValidationProvider : ITokenValidationProvider
    {
        public LoginUserInfo FindUser(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public Task<LoginUserInfo> FindUserAsync(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }

    public class AuthServerValidationProvider : ITokenValidationProvider
    {
        public LoginUserInfo FindUser(HttpContext context)
        {
            throw new NotImplementedException();
        }

        public Task<LoginUserInfo> FindUserAsync(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
