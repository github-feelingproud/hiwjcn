using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc.user;
using Lib.mvc;

namespace Hiwjcn.Core.Infrastructure.Auth
{
    public interface IAuthLoginService
    {
        Task<LoginUserInfo> LoadPermissions(LoginUserInfo model);

        Task<_<LoginUserInfo>> LoginByPassword(string user_name, string password);

        Task<_<LoginUserInfo>> LoginByCode(string phoneOrEmail, string code);

        Task<string> SendOneTimeCode(string phoneOrEmail);

        Task<LoginUserInfo> GetUserInfoByUID(string uid);
    }
}
