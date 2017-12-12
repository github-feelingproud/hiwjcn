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
    public interface IDepartmentServiceBase<DepartmentBase, UserDepartmentBase, DepartmentRoleBase>
        where DepartmentBase : DepartmentEntityBase, new()
        where UserDepartmentBase : UserDepartmentEntityBase, new()
        where DepartmentRoleBase : DepartmentRoleEntityBase, new()
    {
        Task<_<string>> DeleteDepartment(params string[] department_uids);

        Task<_<string>> DeleteDepartmentRecursively(string department_uid);

        Task<_<string>> AddDepartment(DepartmentBase model);

        Task<_<string>> UpdateDepartment(DepartmentBase model);

        Task<List<DepartmentBase>> QueryDepartmentList(string parent = null);

        Task<_<string>> SetUserDepartment(string user_uid, List<UserDepartmentBase> departments);

        Task<_<string>> SetDepartmentRole(string department_uid, List<DepartmentRoleBase> roles);
    }

    public abstract class DepartmentServiceBase<DepartmentBase, UserDepartmentBase, DepartmentRoleBase> :
        IDepartmentServiceBase<DepartmentBase, UserDepartmentBase, DepartmentRoleBase>
        where DepartmentBase : DepartmentEntityBase, new()
        where UserDepartmentBase : UserDepartmentEntityBase, new()
        where DepartmentRoleBase : DepartmentRoleEntityBase, new()

    {
        protected readonly IEFRepository<DepartmentBase> _departmentRepo;
        protected readonly IEFRepository<UserDepartmentBase> _userDepartmentRepo;
        protected readonly IEFRepository<DepartmentRoleBase> _departmentRoleRepo;

        public DepartmentServiceBase(
            IEFRepository<DepartmentBase> _departmentRepo,
            IEFRepository<UserDepartmentBase> _userDepartmentRepo,
            IEFRepository<DepartmentRoleBase> _departmentRoleRepo)
        {
            this._departmentRepo = _departmentRepo;
            this._userDepartmentRepo = _userDepartmentRepo;
            this._departmentRoleRepo = _departmentRoleRepo;
        }


        public virtual async Task<_<string>> DeleteDepartment(params string[] department_uids) =>
            await this._departmentRepo.DeleteByMultipleUIDS_(department_uids);

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
