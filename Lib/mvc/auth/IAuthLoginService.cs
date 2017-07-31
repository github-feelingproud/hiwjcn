using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc.user;

namespace Lib.mvc.auth
{
    public interface IAuthLoginService
    {
        Task<(LoginUserInfo loginuser, string msg)> LoginByPassword(string user_name, string password);

        Task<(LoginUserInfo loginuser, string msg)> LoginByCode(string phoneOrEmail, string code);

        Task<string> SendOneTimeCode(string phoneOrEmail);

        Task<(LoginUserInfo loginuser, string msg)> LoginByToken(string token);

        Task<LoginUserInfo> GetUserInfoByUID(string uid);
    }
}
