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
    public interface IAuthLoginProvider
    {
        Task<_<LoginUserInfo>> LoginByPassword(string user_name, string password);

        Task<_<LoginUserInfo>> LoginByCode(string phoneOrEmail, string code);
        
        Task<_<string>> SendOneTimeCode(string phoneOrEmail);

        Task<LoginUserInfo> GetLoginUserInfoByUserUID(string uid);
    }
}
