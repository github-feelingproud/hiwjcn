using Lib.data.ef;
using Lib.extension;
using Lib.helper;
using Lib.infrastructure.entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc;

namespace Lib.infrastructure.service
{
    public interface IUserLoginServiceBase<UserBase, OneTimeCodeBase, RolePermissionBase, UserRoleBase>
        where UserBase : UserEntityBase, new()
        where OneTimeCodeBase : UserOneTimeCodeEntityBase, new()
        where RolePermissionBase : RolePermissionEntityBase, new()
        where UserRoleBase : UserRoleEntityBase, new()
    {
        Task<_<UserBase>> LoginViaPassword(string user_name, string password);

        Task<_<UserBase>> LoginViaOneTimeCode(string user_name, string code);

        Task<_<UserBase>> RegisterUser(UserBase model);

        Task<List<UserBase>> LoadPermission(List<UserBase> list);

        Task<UserBase> LoadPermission(UserBase model);
    }
    /// <summary>
    /// 用户=角色=权限
    /// </summary>
    /// <typeparam name="UserBase"></typeparam>
    /// <typeparam name="OneTimeCodeBase"></typeparam>
    /// <typeparam name="RolePermissionBase"></typeparam>
    /// <typeparam name="UserRoleBase"></typeparam>
    public abstract class UserLoginServiceBase<UserBase, OneTimeCodeBase, RolePermissionBase, UserRoleBase> :
        IUserLoginServiceBase<UserBase, OneTimeCodeBase, RolePermissionBase, UserRoleBase>
        where UserBase : UserEntityBase, new()
        where OneTimeCodeBase : UserOneTimeCodeEntityBase, new()
        where RolePermissionBase : RolePermissionEntityBase, new()
        where UserRoleBase : UserRoleEntityBase, new()
    {
        protected readonly IEFRepository<UserBase> _userRepo;
        protected readonly IEFRepository<OneTimeCodeBase> _oneTimeCodeRepo;
        protected readonly IEFRepository<RolePermissionBase> _rolePermissionRepo;
        protected readonly IEFRepository<UserRoleBase> _userRoleRepo;

        public UserLoginServiceBase(
            IEFRepository<UserBase> _userRepo,
            IEFRepository<OneTimeCodeBase> _oneTimeCodeRepo,
            IEFRepository<RolePermissionBase> _rolePermissionRepo,
            IEFRepository<UserRoleBase> _userRoleRepo)
        {

            this._userRepo = _userRepo;
            this._oneTimeCodeRepo = _oneTimeCodeRepo;
            this._rolePermissionRepo = _rolePermissionRepo;
            this._userRoleRepo = _userRoleRepo;
        }

        public abstract string EncryptPassword(string password);

        public virtual async Task<_<UserBase>> LoginViaPassword(string user_name, string password)
        {
            var data = new _<UserBase>();
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

            data.SetSuccessData(user_model);
            return data;
        }

        public virtual async Task<_<UserBase>> LoginViaOneTimeCode(string user_name, string code)
        {
            var data = new _<UserBase>();
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

            data.SetSuccessData(user_model);
            return data;
        }

        public virtual async Task<_<UserBase>> RegisterUser(UserBase model)
        {
            var data = new _<UserBase>();
            if (!model.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }

            if (await this._userRepo.AddAsync(model) > 0)
            {
                data.SetSuccessData(model);
                return data;
            }

            throw new Exception("注册失败");
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

    }

    /// <summary>
    /// 用户=部门=角色=权限
    /// </summary>
    /// <typeparam name="UserBase"></typeparam>
    /// <typeparam name="OneTimeCodeBase"></typeparam>
    /// <typeparam name="RolePermissionBase"></typeparam>
    /// <typeparam name="UserRoleBase"></typeparam>
    /// <typeparam name="UserDepartmentBase"></typeparam>
    /// <typeparam name="DepartmentRoleBase"></typeparam>
    public abstract class UserLoginServiceBase<UserBase, OneTimeCodeBase, RolePermissionBase, UserRoleBase, UserDepartmentBase, DepartmentRoleBase> :
        UserLoginServiceBase<UserBase, OneTimeCodeBase, RolePermissionBase, UserRoleBase>
        where UserDepartmentBase : UserDepartmentEntityBase, new()
        where DepartmentRoleBase : DepartmentRoleEntityBase, new()
        where UserBase : UserEntityBase, new()
        where OneTimeCodeBase : UserOneTimeCodeEntityBase, new()
        where RolePermissionBase : RolePermissionEntityBase, new()
        where UserRoleBase : UserRoleEntityBase, new()
    {
        protected readonly IEFRepository<UserDepartmentBase> _userDepartmentRepo;
        protected readonly IEFRepository<DepartmentRoleBase> _departmentRoleRepo;

        public UserLoginServiceBase(
            IEFRepository<UserDepartmentBase> _userDepartmentRepo,
            IEFRepository<DepartmentRoleBase> _departmentRoleRepo,
            IEFRepository<UserBase> _userRepo,
            IEFRepository<OneTimeCodeBase> _oneTimeCodeRepo,
            IEFRepository<RolePermissionBase> _rolePermissionRepo,
            IEFRepository<UserRoleBase> _userRoleRepo) :
            base(_userRepo, _oneTimeCodeRepo, _rolePermissionRepo, _userRoleRepo)
        {
            this._userDepartmentRepo = _userDepartmentRepo;
            this._departmentRoleRepo = _departmentRoleRepo;
        }

        public override async Task<List<UserBase>> LoadPermission(List<UserBase> list)
        {
            if (!ValidateHelper.IsPlumpList(list)) { return list; }

            //load user->role->permission
            list = await base.LoadPermission(list);

            //next load user->department->role->permission
            var user_uids = list.Select(x => x.UID).ToList();

            await this._userRepo.PrepareSessionAsync(async db =>
            {
                var user_query = db.Set<UserBase>().AsNoTrackingQueryable();
                var role_permission_map_query = db.Set<RolePermissionBase>().AsNoTrackingQueryable();
                var user_department_map_query = db.Set<UserDepartmentBase>().AsNoTrackingQueryable();
                var department_role_map_query = db.Set<DepartmentRoleBase>().AsNoTrackingQueryable();

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

                    m.DepartmentIds = user_map.NotEmptyAndDistinct(x => x.department_uid).ToList();

                    m.RoleIds.AddRange(user_map.NotEmptyAndDistinct(x => x.role_uid));
                    m.PermissionIds.AddRange(user_map.NotEmptyAndDistinct(x => x.permission_uid));

                    m.RoleIds = m.RoleIds.Distinct().ToList();
                    m.PermissionIds = m.PermissionIds.Distinct().ToList();
                }
            });

            return list;
        }

    }

}
