using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc.user;
using Lib.infrastructure.entity;
using Lib.infrastructure.entity.auth;

namespace Lib.infrastructure.extension
{
    public static class AuthExtension
    {
        /// <summary>
        /// 设置token信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="loginuser"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static LoginUserInfo SetAuthToken<T>(this LoginUserInfo loginuser, T token)
            where T : AuthTokenBase
        {
            loginuser.LoginToken = token.UID;
            loginuser.RefreshToken = token.RefreshToken;
            loginuser.TokenExpire = token.ExpiryTime;

            return loginuser;
        }
    }
}
