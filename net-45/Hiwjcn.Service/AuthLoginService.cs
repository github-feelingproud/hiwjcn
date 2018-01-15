using Hiwjcn.Bll.User;
using Hiwjcn.Core.Domain.User;
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
    public class AuthLoginService : IAuthLoginService
    {
        private readonly IUserLoginService _login;
        private readonly IUserService _user;

        public AuthLoginService(
            IUserLoginService _login,
            IUserService _user)
        {
            this._login = _login;
            this._user = _user;
        }

        private LoginUserInfo Parse(UserEntity model)
        {
            if (model == null) { return null; }

            var loginuser = new LoginUserInfo()
            {
                IID = model.IID,
                UserID = model.UID,
                UserUID = model.UID,
                NickName = model.NickName,
                UserName = model.UserName,
                HeadImgUrl = model.UserImg,
                Email = model.Email,

                IsActive = model.IsActive,
            };

            return loginuser;
        }

        public async Task<LoginUserInfo> GetLoginUserInfoByUserUID(string uid)
        {
            var user = await this._user.GetUserByUID(uid);
            if (user != null)
            {
                user = await this._login.LoadPermission(user);
            }
            return this.Parse(user);
        }

        public async Task<_<LoginUserInfo>> LoginByCode(string phoneOrEmail, string code)
        {
            var data = new _<LoginUserInfo>();
            var res = await this._login.LoginViaOneTimeCode(phoneOrEmail, code);
            if (res.error)
            {
                data.SetErrorMsg(res.msg);
                return data;
            }

            data.SetSuccessData(this.Parse(res.data));
            return data;
        }

        public async Task<_<LoginUserInfo>> LoginByPassword(string user_name, string password)
        {
            var data = new _<LoginUserInfo>();
            var res = await this._login.LoginViaPassword(user_name, password);
            if (res.error)
            {
                data.SetErrorMsg(res.msg);
                return data;
            }

            data.SetSuccessData(this.Parse(res.data));
            return data;
        }

        public Task<_<string>> SendOneTimeCode(string phoneOrEmail)
        {
            throw new NotImplementedException();
        }
    }
}
