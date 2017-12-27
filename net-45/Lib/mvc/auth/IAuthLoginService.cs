using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc.user;
using Lib.mvc;
using Lib.helper;
using Autofac;

namespace Lib.mvc.auth
{
    public interface IAuthLoginService
    {
        [Obsolete("去掉这个接口")]
        Task<PagerData<LoginUserInfo>> SearchUser(string q = null, int page = 1, int pagesize = 10);

        [Obsolete("出参，入参需要修改" + nameof(UserPermissions))]
        Task<LoginUserInfo> LoadPermissions(LoginUserInfo model);

        Task<_<LoginUserInfo>> LoginByPassword(string user_name, string password);

        Task<_<LoginUserInfo>> LoginByCode(string phoneOrEmail, string code);

        [Obsolete("返回值格式需要修改")]
        Task<string> SendOneTimeCode(string phoneOrEmail);

        Task<LoginUserInfo> GetUserInfoByUID(string uid);
    }
}
