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
using Lib.infrastructure.extension;
using Lib.data.ef;

namespace Lib.infrastructure.service.user
{
    public interface IUserServiceBase<UserBase, UserAvatarBase, OneTimeCodeBase, RoleBase, PermissionBase, RolePermissionBase, UserRoleBase>
        where UserBase : UserEntityBase, new()
        where UserAvatarBase : UserAvatarEntityBase, new()
        where OneTimeCodeBase : UserOneTimeCodeEntityBase, new()
        where RoleBase : RoleEntityBase, new()
        where PermissionBase : PermissionEntityBase, new()
        where RolePermissionBase : RolePermissionEntityBase, new()
        where UserRoleBase : UserRoleEntityBase, new()
    {
        Task<PagerData<UserBase>> QueryUserList(
            string name = null, string email = null, string keyword = null,
            bool load_role = false, int page = 1, int pagesize = 20);

        Task<_<string>> AddUser(UserBase model);

        Task<_<string>> UpdateUser(UserBase model);

        Task<_<string>> ActiveOrDeActiveUser(UserBase model, bool active);

        Task<_<LoginUserInfo>> LoginViaPassword(string user_name, string password, bool load_permission = true);

        Task<_<LoginUserInfo>> LoginViaOneTimeCode(string user_name, string code, bool load_permission = true);

        Task<List<UserBase>> LoadPermission(List<UserBase> list);

        Task<UserBase> LoadPermission(UserBase model);

        Task<_<string>> AddOneTimeCode(OneTimeCodeBase code);

        Task<_<string>> SetUserRoles(string user_uid, List<UserRoleBase> roles);

        Task<_<string>> SetRolePermissions(string role_uid, List<RolePermissionBase> permissions);

        Task<List<RoleBase>> QueryRoleList(string parent = null);

        Task<_<string>> AddRole(RoleBase role);

        Task<_<string>> DeleteRoleRecursively(string role_uid);

        Task<_<string>> DeleteRole(params string[] role_uids);

        Task<_<string>> UpdateRole(RoleBase model);

        Task<List<PermissionBase>> QueryPermissionList(string parent = null);

        Task<_<string>> AddPermission(PermissionBase permission);

        Task<_<string>> UpdatePermission(PermissionBase model);

        Task<_<string>> DeletePermissionRecursively(string permission_uid);

        Task<_<string>> DeletePermission(params string[] permission_uids);
    }

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
        protected readonly IEFRepository<UserBase> _userRepo;
        protected readonly IEFRepository<UserAvatarBase> _userAvatarRepo;
        protected readonly IEFRepository<OneTimeCodeBase> _oneTimeCodeRepo;
        protected readonly IEFRepository<RoleBase> _roleRepo;
        protected readonly IEFRepository<PermissionBase> _permissionRepo;
        protected readonly IEFRepository<RolePermissionBase> _rolePermissionRepo;
        protected readonly IEFRepository<UserRoleBase> _userRoleRepo;

        public UserServiceBase(
            IEFRepository<UserBase> _userRepo,
            IEFRepository<UserAvatarBase> _userAvatarRepo,
            IEFRepository<OneTimeCodeBase> _oneTimeCodeRepo,
            IEFRepository<RoleBase> _roleRepo,
            IEFRepository<PermissionBase> _permissionRepo,
            IEFRepository<RolePermissionBase> _rolePermissionRepo,
            IEFRepository<UserRoleBase> _userRoleRepo)
        {

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
                data.DataList = await this.LoadPermission(data.DataList);
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

        public virtual async Task<_<LoginUserInfo>> LoginViaPassword(string user_name, string password, bool load_permission = true)
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

            if (load_permission)
            {
                user_model = await this.LoadPermission(user_model);
            }

            data.SetSuccessData(this.ParseUser(user_model));
            return data;
        }

        public virtual async Task<_<LoginUserInfo>> LoginViaOneTimeCode(string user_name, string code, bool load_permission = true)
        {
            var data = new _<LoginUserInfo>();
            var user_model = await this._userRepo.GetFirstAsync(x => x.UserName == user_name);
            if (user_model == null)
            {
                data.SetErrorMsg("用户不存在");
                return data;
            }
            var time = DateTime.Now.AddMinutes(-5);
            var code_model = (await this._oneTimeCodeRepo.QueryListAsync(
                where: x => x.UserUID == user_model.UID && x.CreateTime > time,
                orderby: x => x.CreateTime, Desc: true, count: 1)).FirstOrDefault();
            if (code_model?.Code != code)
            {
                data.SetErrorMsg("验证码错误");
                return data;
            }

            if (load_permission)
            {
                user_model = await this.LoadPermission(user_model);
            }

            data.SetSuccessData(this.ParseUser(user_model));
            return data;
        }

        public virtual async Task<List<UserBase>> LoadPermission(List<UserBase> list)
        {
            if (!ValidateHelper.IsPlumpList(list)) { return list; }

            var user_uids = list.Select(x => x.UID).ToList();

            await this._userRepo.PrepareSessionAsync(async db =>
            {
                //table
                var user_query = db.Set<UserBase>().AsNoTrackingQueryable();
                var user_role_map_query = db.Set<UserRoleBase>().AsNoTrackingQueryable();
                var role_permission_map_query = db.Set<RolePermissionBase>().AsNoTrackingQueryable();

                var query = from user in user_query.Where(x => user_uids.Contains(x.UID))
                            join r in user_role_map_query on user.UID equals r.UserID into role_join
                            from role in role_join.DefaultIfEmpty()
                            join p in role_permission_map_query on role.UID equals p.RoleID into permission_join
                            from permission in permission_join.DefaultIfEmpty()

                            select new
                            {
                                user_uid = user.UID,
                                role_uid = role.UID,
                                permission_uid = permission.UID
                            };

                var map = await query.ToListAsync();

                foreach (var m in list)
                {
                    var user_map = map.Where(x => x.user_uid == m.UID);
                    m.RoleIds = user_map.NotEmptyAndDistinct(x => x.role_uid).ToList();
                    m.PermissionIds = user_map.NotEmptyAndDistinct(x => x.permission_uid).ToList();
                }

            });

            return list;
        }

        public virtual async Task<UserBase> LoadPermission(UserBase model) =>
            (await this.LoadPermission(new List<UserBase>() { model })).FirstOrDefault();

        public virtual async Task<_<string>> AddOneTimeCode(OneTimeCodeBase code)
        {
            var data = new _<string>();
            code.Init("code");
            if (!code.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await this._oneTimeCodeRepo.AddAsync(code) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("添加验证码失败");
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

        public virtual async Task<List<RoleBase>> QueryRoleList(string parent = null) =>
            await this._roleRepo.QueryNodeList(parent);

        public virtual async Task<_<string>> AddRole(RoleBase role) => await this._roleRepo.AddTreeNode(role, "role");

        public virtual async Task<_<string>> DeleteRoleRecursively(string role_uid) =>
            await this._roleRepo.DeleteTreeNodeRecursively(role_uid);

        public virtual async Task<_<string>> DeleteRole(params string[] role_uids) =>
            await this._roleRepo.DeleteByMultipleUIDS(role_uids);

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

        public virtual async Task<List<PermissionBase>> QueryPermissionList(string parent = null) =>
            await this._permissionRepo.QueryNodeList(parent);

        public virtual async Task<_<string>> AddPermission(PermissionBase permission) =>
            await this._permissionRepo.AddTreeNode(permission, "per");

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

        public virtual async Task<_<string>> DeletePermissionRecursively(string permission_uid) =>
            await this._permissionRepo.DeleteTreeNodeRecursively(permission_uid);

        public virtual async Task<_<string>> DeletePermission(params string[] permission_uids) =>
            await this._permissionRepo.DeleteByMultipleUIDS(permission_uids);
    }
}
