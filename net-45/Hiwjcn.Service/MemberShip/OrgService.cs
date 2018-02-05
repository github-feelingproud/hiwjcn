using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPC.Core.Entity;
using Lib.data.ef;
using Lib.infrastructure;
using Lib.infrastructure.service;
using Lib.infrastructure.service.user;
using Lib.infrastructure.extension;
using Lib.ioc;
using Lib.helper;
using Lib.mvc;
using Lib.extension;
using Lib.core;
using System.Data.Entity;
using Lib.infrastructure.entity.user;
using EPC.Core;
using Hiwjcn.Core.Domain.User;
using Hiwjcn.Core.Data;

namespace EPC.Service
{
    public interface IOrgService : IAutoRegistered
    {
        Task<List<OrganizationMemberEntity>> GetMyOrgMap(string user_uid);

        Task<List<OrganizationEntity>> GetMyOrgEntity(params string[] user_uid);

        Task<PagerData<UserEntity>> QueryMember(string org_uid, string q, int page, int pagesize);

        Task<_<OrganizationEntity>> AddOrg(OrganizationEntity model);

        Task<_<int>> DeleteOrg(params string[] org_uids);

        Task<_<OrganizationEntity>> UpdateOrg(OrganizationEntity model);

        Task<bool> CheckOwner(string org_uid, string owner_uid);

        Task<OrganizationEntity> GetOrgByUID(string org_uid);

        Task<PagerData<OrganizationEntity>> QueryOrgPager(string q = null, int page = 1, int pagesize = 10);

        Task<_<OrganizationMemberEntity>> AddMember(OrganizationMemberEntity model);

        Task<_<string>> RemoveMember(string org_uid, string user_uid);
    }

    public class OrgService : IOrgService
    {
        private readonly IMSRepository<OrganizationEntity> _orgRepo;
        private readonly IMSRepository<OrganizationMemberEntity> _orgMemberRepo;
        private readonly IMSRepository<UserEntity> _userRepo;

        public OrgService(
            IMSRepository<OrganizationEntity> _orgRepo,
            IMSRepository<OrganizationMemberEntity> _orgMemberRepo,
            IMSRepository<UserEntity> _userRepo)
        {
            this._orgRepo = _orgRepo;
            this._orgMemberRepo = _orgMemberRepo;
            this._userRepo = _userRepo;
        }


        public virtual async Task<_<OrganizationEntity>> AddOrg(OrganizationEntity model)
        {
            var res = new _<OrganizationEntity>();

            model.Init("org");
            if (!model.IsValid(out var msg))
            {
                res.SetErrorMsg(msg);
                return res;
            }

            await this._orgRepo.AddAsync(model);

            res.SetSuccessData(model);
            return res;
        }

        public virtual async Task<_<int>> DeleteOrg(params string[] org_uids) =>
            await this._orgRepo.DeleteByIds(org_uids);

        public virtual async Task<_<OrganizationEntity>> UpdateOrg(OrganizationEntity model)
        {
            var res = new _<OrganizationEntity>();

            var entity = await this._orgRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(entity, "组织不存在");

            this.UpdateOrgEntity(ref entity, ref model);

            entity.Update();
            if (!entity.IsValid(out var msg))
            {
                res.SetErrorMsg(msg);
                return res;
            }

            await this._orgRepo.UpdateAsync(entity);

            res.SetSuccessData(entity);
            return res;
        }

        public virtual async Task<bool> CheckOwner(string org_uid, string owner_uid) =>
            ValidateHelper.IsPlumpString(owner_uid) && (await this.GetOrgByUID(org_uid))?.OwnerUID == owner_uid;

        public virtual async Task<OrganizationEntity> GetOrgByUID(string org_uid) =>
            await this._orgRepo.GetFirstAsync(x => x.UID == org_uid);

        public virtual async Task<PagerData<OrganizationEntity>> QueryOrgPager(string q = null, int page = 1, int pagesize = 10)
        {
            return await this._orgRepo.PrepareIQueryableAsync(async query =>
            {
                if (ValidateHelper.IsPlumpString(q))
                {
                    query = query.Where(x => x.OrgName.StartsWith(q));
                }

                var data = await query.ToPagedListAsync(page, pagesize, x => x.CreateTime);

                if (ValidateHelper.IsPlumpList(data.DataList))
                {
                    var uids = data.DataList.NotEmptyAndDistinct(x => x.OwnerUID).ToList();
                    var list = await this._userRepo.GetListAsync(x => uids.Contains(x.UID));
                    foreach (var m in data.DataList)
                    {
                        m.OwnerModel = list.FirstOrDefault(x => x.UID == m.OwnerUID);
                    }
                }

                return data;
            });
        }

        public virtual async Task<_<OrganizationMemberEntity>> AddMember(OrganizationMemberEntity model)
        {
            await this._orgMemberRepo.DeleteWhereAsync(x => x.UserUID == model.UserUID && x.OrgUID == model.OrgUID);
            return await this._orgMemberRepo.AddEntity_(model, "org-member");
        }

        public virtual async Task<_<string>> RemoveMember(string org_uid, string user_uid)
        {
            var data = new _<string>();
            await this._orgMemberRepo.DeleteWhereAsync(x => x.OrgUID == org_uid && x.UserUID == user_uid);

            data.SetSuccessData(string.Empty);
            return data;
        }

        public async Task<List<OrganizationMemberEntity>> GetMyOrgMap(string user_uid) =>
            await this._orgMemberRepo.GetListAsync(x => x.MemberApproved > 0 && x.OrgApproved > 0 && x.UserUID == user_uid);

        private void UpdateOrgEntity(ref OrganizationEntity old_org, ref OrganizationEntity new_org)
        {
            old_org.OrgName = new_org.OrgName;
            old_org.OrgDescription = new_org.OrgDescription;
            old_org.OrgImage = new_org.OrgImage;
            old_org.OrgWebSite = new_org.OrgWebSite;

            old_org.OwnerUID = new_org.OwnerUID;
        }

        public async Task<PagerData<UserEntity>> QueryMember(string org_uid, string q, int page, int pagesize)
        {
            return await this._orgRepo.PrepareSessionAsync(async db =>
            {
                var map_query = db.Set<OrganizationMemberEntity>().AsNoTrackingQueryable();
                var map = map_query.Where(x => x.OrgUID == org_uid);
                var user_uids = map.Select(x => x.UserUID);
                var query = db.Set<UserEntity>().AsNoTrackingQueryable();
                query = query.Where(x => user_uids.Contains(x.UID));

                if (ValidateHelper.IsPlumpString(q))
                {
                    query = query.Where(x => x.UserName == q || x.NickName.StartsWith(q) || x.UserName.StartsWith(q));
                }
                var data = await query.ToPagedListAsync(page, pagesize, x => x.IID);
                if (ValidateHelper.IsPlumpList(data.DataList))
                {
                    var uids = data.DataList.Select(x => x.UID);
                    var orgs = await map_query.Where(x => uids.Contains(x.UserUID)).ToListAsync();
                    var roles = MemberRoleHelper.GetRoles();
                    foreach (var m in data.DataList)
                    {
                        var org = orgs.Where(x => x.UserUID == m.UID).FirstOrDefault();
                        if (org == null) { continue; }

                        m.OrgFlag = org.Flag;
                        m.OrgUID = org.OrgUID;
                        m.OrgFlagName = string.Join(",", MemberRoleHelper.ParseRoleNames(m.OrgFlag, roles));
                    }
                }

                return data;
            });
        }

        public async Task<List<OrganizationEntity>> GetMyOrgEntity(params string[] user_uid)
        {
            if (!ValidateHelper.IsPlumpList(user_uid))
            {
                return new List<OrganizationEntity>() { };
            }
            user_uid = user_uid.Distinct().ToArray();
            return await this._orgRepo.GetListAsync(x => user_uid.Contains(x.UID));
        }
    }
}
