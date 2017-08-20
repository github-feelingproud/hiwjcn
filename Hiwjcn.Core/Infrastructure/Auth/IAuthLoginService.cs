using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc.user;
using Lib.mvc;
using Lib.helper;

namespace Hiwjcn.Core.Infrastructure.Auth
{
    public interface IAuthLoginService
    {
        Task<PagerData<LoginUserInfo>> SearchUser(string q = null, int page = 1, int pagesize = 10);

        Task<LoginUserInfo> LoadPermissions(LoginUserInfo model);

        Task<_<LoginUserInfo>> LoginByPassword(string user_name, string password);

        Task<_<LoginUserInfo>> LoginByCode(string phoneOrEmail, string code);

        Task<string> SendOneTimeCode(string phoneOrEmail);

        Task<LoginUserInfo> GetUserInfoByUID(string uid);
    }
}
