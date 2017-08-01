using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc.user;
using Lib.mvc;

namespace Lib.mvc.auth
{
    public interface IAuthLoginService
    {
        Task<_<LoginUserInfo>> LoginByPassword(string user_name, string password);

        Task<_<LoginUserInfo>> LoginByCode(string phoneOrEmail, string code);

        Task<string> SendOneTimeCode(string phoneOrEmail);

        Task<_<LoginUserInfo>> LoginByToken(string token);

        Task<LoginUserInfo> GetUserInfoByUID(string uid);
    }
}
