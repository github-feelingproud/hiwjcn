using Lib.helper;
using Lib.infrastructure.service.user;
using Lib.mvc;
using Lib.mvc.auth;
using Lib.mvc.user;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiwjcn.Bll.Auth
{
    public class UserLoginService : IAuthLoginService
    {
        public Task<LoginUserInfo> GetUserInfoByUID(string uid)
        {
            throw new NotImplementedException();
        }

        public Task<UserRolePermission> GetUserRolePermission(string user_uid)
        {
            throw new NotImplementedException();
        }

        public Task<LoginUserInfo> LoadPermissions(LoginUserInfo model)
        {
            throw new NotImplementedException();
        }

        public Task<_<LoginUserInfo>> LoginByCode(string phoneOrEmail, string code)
        {
            throw new NotImplementedException();
        }

        public Task<_<LoginUserInfo>> LoginByPassword(string user_name, string password)
        {
            throw new NotImplementedException();
        }

        public Task<PagerData<LoginUserInfo>> SearchUser(string q = null, int page = 1, int pagesize = 10)
        {
            throw new NotImplementedException();
        }

        public Task<string> SendOneTimeCode(string phoneOrEmail)
        {
            throw new NotImplementedException();
        }
    }
}
