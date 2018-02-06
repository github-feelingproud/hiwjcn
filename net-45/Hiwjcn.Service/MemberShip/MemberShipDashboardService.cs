using EPC.Core.Entity;
using Hiwjcn.Core.Data;
using Lib.data.ef;
using Lib.ioc;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Hiwjcn.Service.MemberShip
{
    public interface IMemberShipDashboardService : IAutoRegistered
    {
        Task<int> MemberCount(string org_uid);
    }

    public class MemberShipDashboardService : IMemberShipDashboardService
    {
        private readonly IMSRepository<OrganizationEntity> _orgRepo;

        public MemberShipDashboardService(
            IMSRepository<OrganizationEntity> _orgRepo)
        {
            this._orgRepo = _orgRepo;
        }

        public async Task<int> MemberCount(string org_uid)
        {
            return await this._orgRepo.PrepareSessionAsync(async db =>
            {
                var query = db.Set<OrganizationMemberEntity>().AsNoTrackingQueryable();

                query = query.Where(x => x.OrgUID == org_uid);

                return await query.CountAsync();
            });
        }

    }
}
