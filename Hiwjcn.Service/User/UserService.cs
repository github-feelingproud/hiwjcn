using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hiwjcn.Core.Domain.User;
using Lib.infrastructure.service;
using Lib.infrastructure.entity;
using Lib.ioc;
using Lib.data.ef;
using Lib.mvc.user;
using Lib.extension;
using Lib.infrastructure.service.user;

namespace Hiwjcn.Bll.User
{
    public interface IUserService :
        IUserServiceBase<UserEntity, UserAvatarEntity>,
        IAutoRegistered
    { }

    public class UserService : UserServiceBase<UserEntity, UserAvatarEntity>,
        IUserService
    {
        public UserService(
            IEFRepository<UserEntity> _userRepo,
            IEFRepository<UserAvatarEntity> _userAvatarRepo) :
            base(_userRepo, _userAvatarRepo)
        { }

        public override void UpdateUserEntity(ref UserEntity old_user, ref UserEntity new_user)
        {
            old_user.Phone = new_user.Phone;
            old_user.QQ = new_user.QQ;
            old_user.UserImg = new_user.UserImg;
            old_user.Email = new_user.Email;
            old_user.Sex = new_user.Sex;
        }
    }
}
