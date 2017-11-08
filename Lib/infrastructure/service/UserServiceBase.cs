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

        public abstract string EncryptPassword(string password);

        public abstract LoginUserInfo ParseUser(UserBase model);

        public virtual async Task<_<LoginUserInfo>> LoginViaPassword(string user_name, string password)
        {
            var data = new _<LoginUserInfo>();
            var user_model = await this._userRepo.GetFirstAsync(x => x.UserName == user_name);
            if (user_model == null)
            {
                data.SetErrorMsg("用户不存在");
                return data;
            }
            if (user_model.PassWord != this.EncryptPassword(password))
            {
                data.SetErrorMsg("密码错误");
                return data;
            }

            data.SetSuccessData(this.ParseUser(user_model));
            return data;
        }

        public virtual async Task<_<LoginUserInfo>> LoginViaOneTimeCode(string user_name, string code)
        {
            var data = new _<LoginUserInfo>();
            var user_model = await this._userRepo.GetFirstAsync(x => x.UserName == user_name);
            if (user_model == null)
            {
                data.SetErrorMsg("用户不存在");
                return data;
            }
            var code_model = (await this._oneTimeCodeRepo.QueryListAsync(where: x => x.UserUID == user_model.UID, orderby: x => x.CreateTime, Desc: true, count: 1)).FirstOrDefault();
            if (code_model?.Code != code)
            {
                data.SetErrorMsg("验证码错误");
                return data;
            }

            data.SetSuccessData(this.ParseUser(user_model));
            return data;
        }

        public virtual async Task<List<UserBase>> LoadPermission(List<UserBase> list)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<_<string>> SetUserRoles(string user_uid, List<UserRoleBase> roles)
        {
            var data = new _<string>();
            if (ValidateHelper.IsPlumpList(roles))
            {
                if (roles.Any(x => x.UserID != user_uid))
                {
                    data.SetErrorMsg("用户ID错误");
                    return data;
                }
            }
            await this._userRoleRepo.DeleteWhereAsync(x => x.UserID == user_uid);
            if (ValidateHelper.IsPlumpList(roles))
            {
                foreach (var m in roles)
                {
                    m.Init("ur");
                    if (!m.IsValid(out var msg))
                    {
                        data.SetErrorMsg(msg);
                        return data;
                    }
                }
                if (await this._userRoleRepo.AddAsync(roles.ToArray()) <= 0)
                {
                    data.SetErrorMsg("保存角色错误");
                    return data;
                }
            }

            data.SetSuccessData(string.Empty);
            return data;
        }

        public virtual async Task<_<string>> SetRolePermissions(string role_uid, List<RolePermissionBase> permissions)
        {
            var data = new _<string>();
            if (ValidateHelper.IsPlumpList(permissions))
            {
                if (permissions.Any(x => x.RoleID != role_uid))
                {
                    data.SetErrorMsg("角色ID错误");
                    return data;
                }
            }
            await this._rolePermissionRepo.DeleteWhereAsync(x => x.RoleID == role_uid);
            if (ValidateHelper.IsPlumpList(permissions))
            {
                foreach (var m in permissions)
                {
                    m.Init("per");
                    if (!m.IsValid(out var msg))
                    {
                        data.SetErrorMsg(msg);
                        return data;
                    }
                }
                if (await this._rolePermissionRepo.AddAsync(permissions.ToArray()) <= 0)
                {
                    data.SetErrorMsg("保存权限错误");
                    return data;
                }
            }

            data.SetSuccessData(string.Empty);
            return data;
        }

        public virtual async Task<PagerData<RoleBase>> QueryRoleList()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<_<string>> AddRole(params RoleBase[] roles)
        {
            if (!ValidateHelper.IsPlumpList(roles)) { throw new Exception("至少有一个权限"); }
            var data = new _<string>();
            foreach (var m in roles)
            {
                if (!m.IsValid(out var msg))
                {
                    data.SetErrorMsg(msg);
                    return data;
                }
            }

            if (await this._roleRepo.AddAsync(roles) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("保存失败");
        }

        public virtual async Task<_<string>> DeleteRole(string role_uid)
        {
            var data = new _<string>();

            var list = await this._roleRepo.GetListEnsureMaxCountAsync(null, 5000, "角色数量达到上限");

            var node = list.Where(x => x.UID == role_uid).FirstOrDefault();
            Com.AssertNotNull(node, $"权限节点为空：{role_uid}");

            var dead_nodes = await list.AsQueryable().FindNodeChildrenRecursively_(node);

            if (await this._roleRepo.DeleteAsync(dead_nodes.ToArray()) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("删除失败");
        }

        public abstract void UpdateRoleEntity(ref RoleBase old_role, ref RoleBase new_role);

        public virtual async Task<_<string>> UpdateRole(RoleBase model)
        {
            var data = new _<string>();

            var role = await this._roleRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(role, $"角色不存在：{model.UID}");
            this.UpdateRoleEntity(ref role, ref model);
            role.Update();
            if (!role.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await this._roleRepo.UpdateAsync(role) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("更新角色失败");
        }

        public virtual async Task<PagerData<PermissionBase>> QueryPermissionList()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<_<string>> AddPermission()
        {
            throw new NotImplementedException();
        }

        public abstract void UpdatePermissionEntity(ref PermissionBase old_permission, ref PermissionBase new_permission);

        public virtual async Task<_<string>> UpdatePermission(PermissionBase model)
        {
            var data = new _<string>();
            var permission = await this._permissionRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(permission, $"权限为空:{model.UID}");
            this.UpdatePermissionEntity(ref permission, ref model);
            permission.Update();
            if (!permission.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await this._permissionRepo.UpdateAsync(permission) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("更新权限错误");
        }

        public virtual async Task<_<string>> DeletePermission(string permission_uid)
        {
            var data = new _<string>();
            var list = await this._permissionRepo.GetListEnsureMaxCountAsync(null, 5000, "权限数量达到上线");
            var permission = list.Where(x => x.UID == permission_uid).FirstOrThrow($"权限不存在{permission_uid}");

            var dead_nodes = await list.AsQueryable().FindNodeChildrenRecursively_(permission);

            if (await this._permissionRepo.DeleteAsync(dead_nodes.ToArray()) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("删除权限错误");
        }
    }
}
