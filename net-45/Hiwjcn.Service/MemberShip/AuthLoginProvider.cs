using Hiwjcn.Bll.User;
using Hiwjcn.Core.Data;
using Hiwjcn.Core.Domain.User;
using Lib.data.ef;
using Lib.extension;
using Lib.helper;
using Lib.infrastructure.service.user;
using Lib.ioc;
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
    public interface IUserLoginService :
        IUserLoginServiceBase<UserEntity, UserOneTimeCodeEntity>,
        IAutoRegistered
    { }

    public class UserLoginService :
        UserLoginServiceBase<UserEntity, UserOneTimeCodeEntity, RolePermissionEntity, UserRoleEntity, UserDepartmentEntity, DepartmentRoleEntity, PermissionEntity>,
        IUserLoginService
    {
        public UserLoginService(
            IMSRepository<UserDepartmentEntity> _userDepartmentRepo,
            IMSRepository<DepartmentRoleEntity> _departmentRoleRepo,
            IMSRepository<UserEntity> _userRepo,
            IMSRepository<UserOneTimeCodeEntity> _oneTimeCodeRepo,
            IMSRepository<RolePermissionEntity> _rolePermissionRepo,
            IMSRepository<UserRoleEntity> _userRoleRepo,
            IMSRepository<PermissionEntity> _perRepo) :
            base(_userDepartmentRepo, _departmentRoleRepo, _userRepo, _oneTimeCodeRepo, _rolePermissionRepo, _userRoleRepo, _perRepo)
        {
            //
        }

        public override string EncryptPassword(string password)
        {
            return password.Trim().ToMD5().Trim().ToUpper();
        }

        public override async Task<_<string>> RegisterCheck(UserEntity model)
        {
            var data = new _<string>();
            if (await this._userRepo.ExistAsync(x => x.UserName == model.UserName))
            {
                data.SetErrorMsg("用户名已存在");
                return data;
            }
            data.SetSuccessData(string.Empty);
            return data;
        }
    }

    public class AuthLoginProvider : IAuthLoginProvider
    {
        private readonly IUserLoginService _login;
        private readonly IUserService _user;

        public AuthLoginProvider(
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
                NickName = model.NickName,
                UserName = model.UserName,
                HeadImgUrl = model.UserImg,
                Email = model.Email,

                Permissions = model.PermissionNames ?? new List<string>() { },
                Roles = model.RoleIds ?? new List<string>() { },
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
            var res = await this._login.ValidUserOneTimeCode(phoneOrEmail, code);
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
            var res = await this._login.ValidUserPassword(user_name, password);
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
