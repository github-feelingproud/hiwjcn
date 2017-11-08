using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;
using Lib.infrastructure;
using Lib.infrastructure.service;
using Model.User;
using WebLogic.Model.User;
using Lib.data;
using Lib.cache;
using Lib.ioc;
using Lib.mvc.user;

namespace Hiwjcn.Bll.User
{
    public interface IUserService :
        IUserServiceBase<UserModel, UserAvatar, UserOneTimeCode, RoleModel, PermissionModel, RolePermissionModel, UserRoleModel>, 
        IAutoRegistered
    { }

    [SingleInstance]
    public class UserService :
        UserServiceBase<UserModel, UserAvatar, UserOneTimeCode, RoleModel, PermissionModel, RolePermissionModel, UserRoleModel>,
        IUserService
    {
        public UserService(
            ICacheProvider _cache,

            IRepository<UserModel> _userRepo,
            IRepository<UserAvatar> _userAvatarRepo,
            IRepository<UserOneTimeCode> _oneTimeCodeRepo,
            IRepository<RoleModel> _roleRepo,
            IRepository<PermissionModel> _permissionRepo,
            IRepository<RolePermissionModel> _rolePermissionRepo,
            IRepository<UserRoleModel> _userRoleRepo) :
            base(_cache, _userRepo, _userAvatarRepo, _oneTimeCodeRepo, _roleRepo, _permissionRepo, _rolePermissionRepo, _userRoleRepo)
        {

        }

        public override string EncryptPassword(string password)
        {
            throw new NotImplementedException();
        }

        public override LoginUserInfo ParseUser(UserModel model)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUserEntity(ref UserModel old_user, ref UserModel new_user)
        {
            throw new NotImplementedException();
        }
    }
}
