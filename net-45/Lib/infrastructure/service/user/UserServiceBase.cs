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
using Lib.infrastructure.entity.user;

namespace Lib.infrastructure.service.user
{
    public interface IUserServiceBase<UserBase>
    {
        Task<UserBase> GetUserByUID(string uid);

        Task<PagerData<UserBase>> QueryUserList(
            string name = null, string email = null, string keyword = null, int page = 1, int pagesize = 20);

        Task<_<UserBase>> UpdateUser(UserBase model);

        Task<_<UserBase>> ActiveOrDeActiveUser(string uid, bool active);
    }

    public abstract class UserServiceBase<UserBase> :
        IUserServiceBase<UserBase>
        where UserBase : UserEntityBase, new()
    {
        protected readonly IEFRepository<UserBase> _userRepo;

        public UserServiceBase(
            IEFRepository<UserBase> _userRepo)
        {

            this._userRepo = _userRepo;
        }

        public virtual async Task<PagerData<UserBase>> QueryUserList(
            string name = null, string email = null, string keyword = null, int page = 1, int pagesize = 20)
        {
            return await this._userRepo.PrepareIQueryableAsync(async query =>
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

                return await query.ToPagedListAsync(page, pagesize, x => x.CreateTime);
            });
        }

        public abstract void UpdateUserEntity(ref UserBase old_user, ref UserBase new_user);

        public virtual async Task<_<UserBase>> UpdateUser(UserBase model)
        {
            var data = new _<UserBase>();

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
                data.SetSuccessData(user);
                return data;
            }

            throw new Exception("更新失败");
        }

        public virtual async Task<_<UserBase>> ActiveOrDeActiveUser(string uid, bool active)
        {
            var data = new _<UserBase>();

            var user = await this._userRepo.GetFirstAsync(x => x.UID == uid);
            Com.AssertNotNull(user, $"用户不存在:{uid}");

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
                data.SetSuccessData(user);
                return data;
            }

            throw new Exception("操作失败");
        }

        public async Task<UserBase> GetUserByUID(string uid) =>
            await this._userRepo.GetFirstAsync(x => x.UID == uid && x.IsRemove <= 0);
    }
}
