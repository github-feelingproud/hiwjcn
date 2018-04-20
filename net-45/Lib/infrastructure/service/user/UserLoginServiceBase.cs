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
using Lib.infrastructure.entity.user;
using Lib.mvc;

namespace Lib.infrastructure.service.user
{
    public interface IUserLoginServiceBase<UserBase, OneTimeCodeBase>
    {
        Task<_<UserBase>> ValidUserPassword(string user_name, string password);

        Task<_<UserBase>> ValidUserOneTimeCode(string user_name, string code);

        Task<_<OneTimeCodeBase>> AddOneTimeCode(string user_name, OneTimeCodeBase code);

        Task<_<UserBase>> RegisterUser(UserBase model);

        Task<_<UserBase>> ChangePwd(UserBase model);

        Task<UserBase> LoadPermission(UserBase model);

        Task<UserBase> GetUserByUID(string uid);
    }

    /// <summary>
    /// 用户=角色=权限
    /// </summary>
    public abstract class UserLoginServiceBase<UserBase, OneTimeCodeBase, RolePermissionBase, UserRoleBase, PermissionBase> :
        IUserLoginServiceBase<UserBase, OneTimeCodeBase>
        where UserBase : UserEntityBase, new()
        where OneTimeCodeBase : UserOneTimeCodeEntityBase, new()
        where RolePermissionBase : RolePermissionEntityBase, new()
        where UserRoleBase : UserRoleEntityBase, new()
        where PermissionBase : PermissionEntityBase, new()
    {
        protected readonly IEFRepository<UserBase> _userRepo;
        protected readonly IEFRepository<OneTimeCodeBase> _oneTimeCodeRepo;
        protected readonly IEFRepository<RolePermissionBase> _rolePermissionRepo;
        protected readonly IEFRepository<UserRoleBase> _userRoleRepo;
        protected readonly IEFRepository<PermissionBase> _perRepo;

        public UserLoginServiceBase(
            IEFRepository<UserBase> _userRepo,
            IEFRepository<OneTimeCodeBase> _oneTimeCodeRepo,
            IEFRepository<RolePermissionBase> _rolePermissionRepo,
            IEFRepository<UserRoleBase> _userRoleRepo,
            IEFRepository<PermissionBase> _perRepo)
        {

            this._userRepo = _userRepo;
            this._oneTimeCodeRepo = _oneTimeCodeRepo;
            this._rolePermissionRepo = _rolePermissionRepo;
            this._userRoleRepo = _userRoleRepo;
            this._perRepo = _perRepo;
        }

        public abstract string EncryptPassword(string password);

        public virtual async Task<_<UserBase>> ValidUserPassword(string user_name, string password)
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

        public virtual async Task<_<UserBase>> ValidUserOneTimeCode(string user_name, string code)
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

        public abstract Task<_<string>> RegisterCheck(UserBase model);

        public virtual async Task<_<UserBase>> RegisterUser(UserBase model)
        {
            var data = new _<UserBase>();

            model.Init("user");
            model.PassWord = this.EncryptPassword(model.PassWord ?? throw new Exception("密码为空"));
            if (!model.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            var check = await this.RegisterCheck(model);
            if (check.error)
            {
                data.SetErrorMsg(check.msg);
                return data;
            }

            if (await this._userRepo.AddAsync(model) > 0)
            {
                data.SetSuccessData(model);
                return data;
            }

            throw new Exception("注册失败");
        }

        public virtual async Task<UserBase> LoadPermission(UserBase model)
        {
            if (model == null) { throw new ArgumentNullException(nameof(UserBase)); }

            await this._userRepo.PrepareSessionAsync(async db =>
            {
                //table
                var user_role_map_query = db.Set<UserRoleBase>().AsNoTrackingQueryable();
                var role_permission_map_query = db.Set<RolePermissionBase>().AsNoTrackingQueryable();
                var per_query = db.Set<PermissionBase>().AsNoTrackingQueryable();

                var roleids = user_role_map_query.Where(x => x.UserID == model.UID).Select(x => x.RoleID);
                var perids = role_permission_map_query.Where(x => roleids.Contains(x.RoleID)).Select(x => x.PermissionID).Distinct();

                var pers = await per_query.Where(x => perids.Contains(x.UID)).ToListAsync();

                model.PermissionIds = pers.NotEmptyAndDistinct(x => x.UID).ToList();
                model.PermissionNames = pers.NotEmptyAndDistinct(x => x.Name).ToList();
            });

            return model;
        }

        public virtual async Task<_<UserBase>> ChangePwd(UserBase model)
        {
            var data = new _<UserBase>();

            var user = await this._userRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(user, "用户不存在，无法修改密码");
            user.PassWord = this.EncryptPassword(model.PassWord);
            user.Update();
            if (!user.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await this._userRepo.UpdateAsync(user) > 0)
            {
                data.SetSuccessData(user);
                return data;
            }

            throw new Exception("密码修改失败");
        }

        public virtual async Task<_<OneTimeCodeBase>> AddOneTimeCode(string user_name, OneTimeCodeBase code)
        {
            var data = new _<OneTimeCodeBase>();

            code.Init("code");
            if (!code.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }

            await this._oneTimeCodeRepo.AddAsync(code);

            data.SetSuccessData(code);
            return data;
        }

        public async Task<UserBase> GetUserByUID(string uid) => await this._userRepo.GetFirstAsync(x => x.UID == uid);
    }
}
