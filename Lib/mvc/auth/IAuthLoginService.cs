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
        Task<LoginUserInfo> LoginByPassword(string user_name, string password);

        Task<LoginUserInfo> LoginByCode(string phoneOrEmail, string code);

        Task<string> SendOneTimeCode(string phoneOrEmail);

        Task<LoginUserInfo> LoginByToken(string token);

        Task<LoginUserInfo> GetUserInfoByUID(string uid);

        Task<List<string>> GetUserPermission(string uid);
    }
}
