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
using Lib.infrastructure.extension;
using Lib.data.ef;

namespace Lib.infrastructure.service.organization
{
    public interface IOrganizationServiceBase<OrgBase, OrgMemberBase>
        where OrgBase : OrganizationEntityBase, new()
        where OrgMemberBase : OrganizationMemberEntityBase, new()
    { }

    public abstract class OrganizationServiceBase<OrgBase, OrgMemberBase> :
        IOrganizationServiceBase<OrgBase, OrgMemberBase>
        where OrgBase : OrganizationEntityBase, new()
        where OrgMemberBase : OrganizationMemberEntityBase, new()
    {
        protected readonly IEFRepository<OrgBase> _orgRepo;
        protected readonly IEFRepository<OrgMemberBase> _orgMemberRepo;

        public OrganizationServiceBase(
            IEFRepository<OrgBase> _orgRepo,
            IEFRepository<OrgMemberBase> _orgMemberRepo)
        {
            this._orgRepo = _orgRepo;
            this._orgMemberRepo = _orgMemberRepo;
        }

        public virtual async Task<_<string>> AddOrg(OrgBase model) =>
            await this._orgRepo.AddEntity_(model, "org");

        public virtual async Task<_<string>> DeleteOrg(params string[] org_uids) =>
            await this._orgRepo.DeleteByMultipleUIDS_(org_uids);

        public abstract void UpdateOrgEntity(ref OrgBase old_org, ref OrgBase new_org);

        public virtual async Task<_<string>> UpdateOrg(OrgBase model)
        {
            var data = new _<string>();
            var org = await this._orgRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(org, "组织不存在");
            this.UpdateOrgEntity(ref org, ref model);
            org.Update();
            if (!org.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await this._orgRepo.UpdateAsync(org) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("更新组织失败");
        }

        public virtual async Task<bool> CheckOwner(string org_uid, string owner_uid) =>
            ValidateHelper.IsPlumpString(owner_uid) && (await this.GetOrgByUID(org_uid))?.OwnerUID == owner_uid;

        public virtual async Task<OrgBase> GetOrgByUID(string org_uid) =>
            await this._orgRepo.GetFirstAsync(x => x.UID == org_uid);

        public virtual async Task<PagerData<OrgBase>> QueryOrgPager(string q = null, int page = 1, int pagesize = 10)
        {
            return await this._orgRepo.PrepareIQueryableAsync_(async query =>
            {
                var data = new PagerData<OrgBase>();

                if (ValidateHelper.IsPlumpString(q))
                {
                    query = query.Where(x => x.OrgName.StartsWith(q));
                }

                data.ItemCount = await query.CountAsync();
                data.DataList = await query.OrderByDescending(x => x.CreateTime).QueryPage(page, pagesize).ToListAsync();
                return data;
            });
        }

        public virtual async Task<_<string>> AddMember(OrgMemberBase model) =>
            await this._orgMemberRepo.AddEntity_(model, "org-member");

        public virtual async Task<_<string>> RemoveMember(string org_uid, string user_uid)
        {
            var data = new _<string>();
            await this._orgMemberRepo.DeleteWhereAsync(x => x.OrgUID == org_uid && x.UserUID == user_uid);

            data.SetSuccessData(string.Empty);
            return data;
        }

    }
}
