using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;
using Lib.data;
using Lib.mvc;
using Lib.helper;
using Lib.extension;
using System.Data.Entity;
using Lib.core;
using Lib.mvc.user;
using Lib.cache;

namespace Lib.infrastructure.service
{
    public interface IUserServiceBase<UserBase, UserAvatarBase, OneTimeCodeBase, RoleBase, PermissionBase, RolePermissionBase, UserRoleBase>
        where UserBase : UserEntityBase, new()
        where UserAvatarBase : UserAvatarEntityBase, new()
        where OneTimeCodeBase : UserOneTimeCodeEntityBase, new()
        where RoleBase : RoleEntityBase, new()
        where PermissionBase : PermissionEntityBase, new()
        where RolePermissionBase : RolePermissionEntityBase, new()
        where UserRoleBase : UserRoleEntityBase, new()
    { }

    public abstract class UserServiceBase<UserBase, UserAvatarBase, OneTimeCodeBase, RoleBase, PermissionBase, RolePermissionBase, UserRoleBase> :
        IUserServiceBase<UserBase, UserAvatarBase, OneTimeCodeBase, RoleBase, PermissionBase, RolePermissionBase, UserRoleBase>
        where UserBase : UserEntityBase, new()
        where UserAvatarBase : UserAvatarEntityBase, new()
        where OneTimeCodeBase : UserOneTimeCodeEntityBase, new()
        where RoleBase : RoleEntityBase, new()
        where PermissionBase : PermissionEntityBase, new()
        where RolePermissionBase : RolePermissionEntityBase, new()
        where UserRoleBase : UserRoleEntityBase, new()
    {
        protected readonly ICacheProvider _cache;

        protected readonly IRepository<UserBase> _userRepo;
        protected readonly IRepository<UserAvatarBase> _userAvatarRepo;
        protected readonly IRepository<OneTimeCodeBase> _oneTimeCodeRepo;
        protected readonly IRepository<RoleBase> _roleRepo;
        protected readonly IRepository<PermissionBase> _permissionRepo;
        protected readonly IRepository<RolePermissionBase> _rolePermissionRepo;
        protected readonly IRepository<UserRoleBase> _userRoleRepo;

        public UserServiceBase(
            ICacheProvider _cache,

            IRepository<UserBase> _userRepo,
            IRepository<UserAvatarBase> _userAvatarRepo,
            IRepository<OneTimeCodeBase> _oneTimeCodeRepo,
            IRepository<RoleBase> _roleRepo,
            IRepository<PermissionBase> _permissionRepo,
            IRepository<RolePermissionBase> _rolePermissionRepo,
            IRepository<UserRoleBase> _userRoleRepo)
        {
            this._cache = _cache;

            this._userRepo = _userRepo;
            this._userAvatarRepo = _userAvatarRepo;
            this._oneTimeCodeRepo = _oneTimeCodeRepo;
            this._roleRepo = _roleRepo;
            this._permissionRepo = _permissionRepo;
            this._rolePermissionRepo = _rolePermissionRepo;
            this._userRoleRepo = _userRoleRepo;
        }

        public virtual async Task<PagerData<UserBase>> QueryUserList(
            string name = null, string email = null, string keyword = null,
            bool load_role = false, int page = 1, int pagesize = 20)
        {
            var data = new PagerData<UserBase>();

            await this._userRepo.PrepareIQueryableAsync(async query =>
            {
                if (ValidateHelper.IsPlumpString(name))
                {
                    query = query.Where(x => x.NickName == name);
                }
                if (ValidateHelper.IsPlumpString(email))
                {
                    query = query.Where(x => x.Email == email);
                }
                if (ValidateHelper.IsPlumpString(keyword))
                {
                    query = query.Where(x =>
                    x.NickName.Contains(keyword)
                    || x.Phone.Contains(keyword)
                    || x.Email.Contains(keyword));
                }

                data.ItemCount = await query.CountAsync();
                data.DataList = await query.OrderByDescending(x => x.UpdateTime).QueryPage(page, pagesize).ToListAsync();
            });

            if (ValidateHelper.IsPlumpList(data.DataList) && load_role)
            {
                //load role
            }

            return data;
        }

        public virtual async Task<_<string>> AddUser(UserBase model)
        {
            var data = new _<string>();
            if (!model.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }

            if (await this._userRepo.AddAsync(model) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("注册失败");
        }

        public abstract void UpdateUserEntity(ref UserBase old_user, ref UserBase new_user);

        public virtual async Task<_<string>> UpdateUser(UserBase model)
        {
            var data = new _<string>();

            var user = await this._userRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(user, $"用户不存在:{model.UID}");
            this.UpdateUserEntity(ref user, ref model);
            user.Update();
            if (!user.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }

            if (await this._userRepo.UpdateAsync(user) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("更新失败");
        }

        public virtual async Task<_<string>> ActiveOrDeActiveUser(UserBase model, bool active)
        {
            var data = new _<string>();

            var user = await this._userRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(user, $"用户不存在:{model.UID}");

            if (user.IsActive.ToBool() == active)
            {
                data.SetErrorMsg("状态不需要改变");
                return data;
            }
            user.IsActive = active.ToBoolInt();
            user.Update();
            if (!user.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await this._userRepo.UpdateAsync(user) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("操作失败");
        }

        public virtual async Task<_<LoginUserInfo>> LoginViaPassword()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<_<LoginUserInfo>> LoginViaOneTimeCode()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<_<string>> SetUserRoles()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<_<string>> SetRolePermissions()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<PagerData<RoleBase>> QueryRoleList()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<_<string>> AddRole()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<_<string>> DeleteRole()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<_<string>> UpdateRole()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<PagerData<PermissionBase>> QueryPermissionList()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<_<string>> AddPermission()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<_<string>> UpdatePermission()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<_<string>> DeletePermission()
        {
            throw new NotImplementedException();
        }
    }
}
