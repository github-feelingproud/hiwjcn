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
using Lib.helper;
using System.Data.Entity;
using Hiwjcn.Core.Data;

namespace Hiwjcn.Service.MemberShip
{
    public interface IUserService : IUserServiceBase<UserEntity>,
        IAutoRegistered
    {
        Task<UserEntity> GetUserByName(string name);

        Task<List<UserEntity>> UserSuggest(string q, int count);
    }

    public class UserService : UserServiceBase<UserEntity>,
        IUserService
    {
        private readonly IMSRepository<UserRoleEntity> _userRoleRepo;
        private readonly IMSRepository<RoleEntity> _roleRepo;

        public UserService(
            IMSRepository<UserEntity> _userRepo,
            IMSRepository<UserRoleEntity> _userRoleRepo,
            IMSRepository<RoleEntity> _roleRepo) :
            base(_userRepo)
        {
            this._userRoleRepo = _userRoleRepo;
            this._roleRepo = _roleRepo;
        }

        public override void UpdateUserEntity(ref UserEntity old_user, ref UserEntity new_user)
        {
            old_user.Flag = new_user.Flag;
            old_user.Phone = new_user.Phone;
            old_user.UserImg = new_user.UserImg;
            old_user.NickName = new_user.NickName;
            old_user.IsRemove = new_user.IsRemove;
        }

        public override async Task<PagerData<UserEntity>> QueryUserList(string name = null, string email = null, string keyword = null, int page = 1, int pagesize = 20)
        {
            var data = await base.QueryUserList(name, email, keyword, page, pagesize);
            if (ValidateHelper.IsPlumpList(data.DataList))
            {
                var uids = data.DataList.Select(x => x.UID).ToList();
                var maps = await this._userRoleRepo.GetListAsync(x => uids.Contains(x.UserID));
                if (ValidateHelper.IsPlumpList(maps))
                {
                    var role_uids = maps.Select(x => x.RoleID).ToList();
                    var roles = await this._roleRepo.GetListAsync(x => role_uids.Contains(x.UID));

                    foreach (var m in data.DataList)
                    {
                        m.RoleIds = maps.Where(x => x.UserID == m.UID).Select(x => x.RoleID).ToList();
                        m.RoleNames = ",".Join_(roles.Where(x => m.RoleIds.Contains(x.UID)).Select(x => x.RoleName));
                    }
                }
            }
            return data;
        }

        public async Task<List<UserEntity>> UserSuggest(string q, int count)
        {
            if (!ValidateHelper.IsPlumpString(q))
            {
                return new List<UserEntity>() { };
            }
            return await this._userRepo.PrepareIQueryableAsync(async query =>
            {
                query = query.Where(x =>
                x.UserName.StartsWith(q) || x.NickName.StartsWith(q) ||
                x.Phone == q || x.Email == q);
                return await query.OrderByDescending(x => x.IID).Take(count).ToListAsync();
            });
        }

        public async Task<UserEntity> GetUserByName(string name) =>
            await this._userRepo.GetFirstAsync(x => x.UserName == name);
    }
}
