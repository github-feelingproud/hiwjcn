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
        IUserServiceWithDepartmentBase<
            DepartmentEntity, UserDepartmentEntity, DepartmentRoleEntity,
            UserEntity, UserAvatarEntity, UserOneTimeCodeEntity,
            RoleEntity, PermissionEntity, RolePermissionEntity, UserRoleEntity>,
        IAutoRegistered
    { }

    public class UserService :
        UserServiceWithDepartmentBase<
            DepartmentEntity, UserDepartmentEntity, DepartmentRoleEntity,
            UserEntity, UserAvatarEntity, UserOneTimeCodeEntity,
            RoleEntity, PermissionEntity, RolePermissionEntity, UserRoleEntity>, IUserService
    {
        public UserService(
            IEFRepository<DepartmentEntity> _departmentRepo,
            IEFRepository<UserDepartmentEntity> _userDepartmentRepo,
            IEFRepository<DepartmentRoleEntity> _departmentRoleRepo,
            IEFRepository<UserEntity> _userRepo,
            IEFRepository<UserAvatarEntity> _userAvatarRepo,
            IEFRepository<UserOneTimeCodeEntity> _oneTimeCodeRepo,
            IEFRepository<RoleEntity> _roleRepo,
            IEFRepository<PermissionEntity> _permissionRepo,
            IEFRepository<RolePermissionEntity> _rolePermissionRepo,
            IEFRepository<UserRoleEntity> _userRoleRepo) :
            base(_departmentRepo, _userDepartmentRepo, _departmentRoleRepo, _userRepo, _userAvatarRepo, _oneTimeCodeRepo, _roleRepo, _permissionRepo, _rolePermissionRepo, _userRoleRepo)
        { }

        public override string EncryptPassword(string password)
        {
            return password.Trim().ToMD5().Trim().ToLower();
        }

        public override LoginUserInfo ParseUser(UserEntity model)
        {
            return new LoginUserInfo() { };
        }

        public override void UpdateDepartmentEntity(ref DepartmentEntity old_department, ref DepartmentEntity new_department)
        {
            old_department.DepartmentName = new_department.DepartmentName;
            old_department.Description = new_department.Description;
        }

        public override void UpdatePermissionEntity(ref PermissionEntity old_permission, ref PermissionEntity new_permission)
        {
            old_permission.Description = new_permission.Description;
        }

        public override void UpdateRoleEntity(ref RoleEntity old_role, ref RoleEntity new_role)
        {
            old_role.RoleName = new_role.RoleName;
        }

        public override void UpdateUserEntity(ref UserEntity old_user, ref UserEntity new_user)
        {
            old_user.NickName = new_user.NickName;
        }
    }
}
