using Hiwjcn.Core.Data;
using Hiwjcn.Core.Domain.User;
using Lib.extension;
using Lib.infrastructure.service.user;
using Lib.ioc;
using Lib.mvc;
using System.Threading.Tasks;

namespace Hiwjcn.Service.MemberShip
{
    public interface IUserLoginService :
        IUserLoginServiceBase<UserEntity, UserOneTimeCodeEntity>,
        IAutoRegistered
    { }

    public class UserLoginService :
        UserLoginServiceBase<UserEntity, UserOneTimeCodeEntity, RolePermissionEntity, UserRoleEntity, PermissionEntity>,
        IUserLoginService
    {
        public UserLoginService(
            IMSRepository<UserEntity> _userRepo,
            IMSRepository<UserOneTimeCodeEntity> _oneTimeCodeRepo,
            IMSRepository<RolePermissionEntity> _rolePermissionRepo,
            IMSRepository<UserRoleEntity> _userRoleRepo,
            IMSRepository<PermissionEntity> _perRepo) :
            base(_userRepo, _oneTimeCodeRepo, _rolePermissionRepo, _userRoleRepo, _perRepo)
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
}
