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

namespace Lib.infrastructure.service
{
    //UserServiceWithDepartmentBase
    public interface IUserServiceWithDepartmentBase<DepartmentBase, UserDepartmentBase, DepartmentRoleBase, UserBase, UserAvatarBase, OneTimeCodeBase, RoleBase, PermissionBase, RolePermissionBase, UserRoleBase> :
        IUserServiceBase<UserBase, UserAvatarBase, OneTimeCodeBase, RoleBase, PermissionBase, RolePermissionBase, UserRoleBase>
        where DepartmentBase : DepartmentEntityBase, new()
        where UserDepartmentBase : UserDepartmentEntityBase, new()
        where DepartmentRoleBase : DepartmentRoleEntityBase, new()

        where UserBase : UserEntityBase, new()
        where UserAvatarBase : UserAvatarEntityBase, new()
        where OneTimeCodeBase : UserOneTimeCodeEntityBase, new()
        where RoleBase : RoleEntityBase, new()
        where PermissionBase : PermissionEntityBase, new()
        where RolePermissionBase : RolePermissionEntityBase, new()
        where UserRoleBase : UserRoleEntityBase, new()
    {
        Task<_<string>> DeleteDepartment(params string[] department_uids);

        Task<_<string>> DeleteDepartmentRecursively(string department_uid);

        Task<_<string>> AddDepartment(DepartmentBase model);

        Task<_<string>> UpdateDepartment(DepartmentBase model);

        Task<List<DepartmentBase>> QueryDepartmentList(string parent = null);

        Task<_<string>> SetUserDepartment(string user_uid, List<UserDepartmentBase> departments);

        Task<_<string>> SetDepartmentRole(string department_uid, List<DepartmentRoleBase> roles);
    }

    public abstract class UserServiceWithDepartmentBase<DepartmentBase, UserDepartmentBase, DepartmentRoleBase, UserBase, UserAvatarBase, OneTimeCodeBase, RoleBase, PermissionBase, RolePermissionBase, UserRoleBase> :
        UserServiceBase<UserBase, UserAvatarBase, OneTimeCodeBase, RoleBase, PermissionBase, RolePermissionBase, UserRoleBase>,
        IUserServiceWithDepartmentBase<DepartmentBase, UserDepartmentBase, DepartmentRoleBase, UserBase, UserAvatarBase, OneTimeCodeBase, RoleBase, PermissionBase, RolePermissionBase, UserRoleBase>
        where DepartmentBase : DepartmentEntityBase, new()
        where UserDepartmentBase : UserDepartmentEntityBase, new()
        where DepartmentRoleBase : DepartmentRoleEntityBase, new()
        where UserBase : UserEntityBase, new()
        where UserAvatarBase : UserAvatarEntityBase, new()
        where OneTimeCodeBase : UserOneTimeCodeEntityBase, new()
        where RoleBase : RoleEntityBase, new()
        where PermissionBase : PermissionEntityBase, new()
        where RolePermissionBase : RolePermissionEntityBase, new()
        where UserRoleBase : UserRoleEntityBase, new()
    {
        protected readonly IEFRepository<DepartmentBase> _departmentRepo;
        protected readonly IEFRepository<UserDepartmentBase> _userDepartmentRepo;
        protected readonly IEFRepository<DepartmentRoleBase> _departmentRoleRepo;

        public UserServiceWithDepartmentBase(
            IEFRepository<DepartmentBase> _departmentRepo,
            IEFRepository<UserDepartmentBase> _userDepartmentRepo,
            IEFRepository<DepartmentRoleBase> _departmentRoleRepo,
            IEFRepository<UserBase> _userRepo,
            IEFRepository<UserAvatarBase> _userAvatarRepo,
            IEFRepository<OneTimeCodeBase> _oneTimeCodeRepo,
            IEFRepository<RoleBase> _roleRepo,
            IEFRepository<PermissionBase> _permissionRepo,
            IEFRepository<RolePermissionBase> _rolePermissionRepo,
            IEFRepository<UserRoleBase> _userRoleRepo) :
            base(_userRepo, _userAvatarRepo, _oneTimeCodeRepo, _roleRepo, _permissionRepo, _rolePermissionRepo, _userRoleRepo)
        {
            this._departmentRepo = _departmentRepo;
            this._userDepartmentRepo = _userDepartmentRepo;
            this._departmentRoleRepo = _departmentRoleRepo;
        }

        public override async Task<List<UserBase>> LoadPermission(List<UserBase> list)
        {
            //load user->role->permission
            list = await base.LoadPermission(list);

            //next load user->department->role->permission
            var user_uids = list.Select(x => x.UID);

            await this._userRepo.PrepareSessionAsync(async db =>
            {
                var user_query = db.Set<UserBase>().AsNoTrackingQueryable();
                var role_permission_map_query = db.Set<RolePermissionBase>().AsNoTrackingQueryable();
                var role_query = db.Set<RoleBase>().AsNoTrackingQueryable();
                var user_department_map_query = db.Set<UserDepartmentBase>().AsNoTrackingQueryable();
                var department_role_map_query = db.Set<DepartmentRoleBase>().AsNoTrackingQueryable();
                var department_query = db.Set<DepartmentBase>().AsNoTrackingQueryable();
                var permission_query = db.Set<PermissionBase>().AsNoTrackingQueryable();

                var query = from user in user_query.Where(x => user_uids.Contains(x.UID))

                            join d in user_department_map_query on user.UID equals d.UserUID into dept_join
                            from dept in dept_join.DefaultIfEmpty()

                            join r in department_role_map_query on dept.UID equals r.DepartmentUID into role_join
                            from role in role_join.DefaultIfEmpty()

                            join p in role_permission_map_query on role.UID equals p.RoleID into permission_join
                            from permission in permission_join.DefaultIfEmpty()

                            select new
                            {
                                user_uid = user.UID,
                                department_uid = dept.UID,
                                role_uid = role.UID,
                                permission_uid = permission.UID
                            };

                var map = await query.ToListAsync();

                foreach (var m in list)
                {
                    var user_map = map.Where(x => x.user_uid == m.UID);

                    m.DepartmentIds = new List<string>();
                    m.DepartmentIds.AddRange(user_map.NotEmptyAndDistinct(x => x.department_uid));
                    m.RoleIds.AddRange(user_map.NotEmptyAndDistinct(x => x.role_uid));
                    m.PermissionIds.AddRange(user_map.NotEmptyAndDistinct(x => x.permission_uid));
                }
            });

            return list;
        }

        public virtual async Task<_<string>> DeleteDepartment(params string[] department_uids) =>
            await this._departmentRepo.DeleteByMultipleUIDS(department_uids);

        public virtual async Task<_<string>> DeleteDepartmentRecursively(string department_uid) =>
            await this._departmentRepo.DeleteTreeNodeRecursively(department_uid);

        public virtual async Task<_<string>> AddDepartment(DepartmentBase model) =>
            await this._departmentRepo.AddTreeNode(model, "dept");

        public abstract void UpdateDepartmentEntity(ref DepartmentBase old_department, ref DepartmentBase new_department);

        public virtual async Task<_<string>> UpdateDepartment(DepartmentBase model)
        {
            var data = new _<string>();
            var department = await this._departmentRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(department, "部门不存在");
            this.UpdateDepartmentEntity(ref department, ref model);
            department.Update();

            if (await this._departmentRepo.UpdateAsync(department) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("更新部门失败");
        }

        public virtual async Task<List<DepartmentBase>> QueryDepartmentList(string parent = null) =>
            await this._departmentRepo.QueryNodeList(parent);

        public virtual async Task<_<string>> SetUserDepartment(string user_uid, List<UserDepartmentBase> departments)
        {
            var data = new _<string>();
            if (ValidateHelper.IsPlumpList(departments))
            {
                if (departments.Any(x => x.UserUID != user_uid))
                {
                    data.SetErrorMsg("分配部门参数错误");
                    return data;
                }
                foreach (var m in departments)
                {
                    m.Init("user_dept");
                    if (!m.IsValid(out var msg))
                    {
                        data.SetErrorMsg(msg);
                        return data;
                    }
                }
            }

            await this._userDepartmentRepo.DeleteWhereAsync(x => x.UserUID == user_uid);

            if (ValidateHelper.IsPlumpList(departments))
            {
                if (await this._userDepartmentRepo.AddAsync(departments.ToArray()) <= 0)
                {
                    throw new Exception("设置部门失败");
                }
            }

            data.SetSuccessData(string.Empty);
            return data;
        }

        public virtual async Task<_<string>> SetDepartmentRole(string department_uid, List<DepartmentRoleBase> roles)
        {
            var data = new _<string>();
            if (ValidateHelper.IsPlumpList(roles))
            {
                if (roles.Any(x => x.DepartmentUID != department_uid))
                {
                    data.SetErrorMsg("设置角色参数错误");
                    return data;
                }
                foreach (var m in roles)
                {
                    m.Init("dept_role");
                    if (!m.IsValid(out var msg))
                    {
                        data.SetErrorMsg(msg);
                        return data;
                    }
                }
            }

            await this._departmentRoleRepo.DeleteWhereAsync(x => x.DepartmentUID == department_uid);

            if (ValidateHelper.IsPlumpList(roles))
            {
                if (await this._departmentRoleRepo.AddAsync(roles.ToArray()) <= 0)
                {
                    throw new Exception("保存角色失败");
                }
            }

            data.SetSuccessData(string.Empty);
            return data;
        }
    }

}
