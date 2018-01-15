using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure;
using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Core.Data.Auth;
using Lib.data;
using Lib.core;
using Lib.extension;
using Lib.helper;
using System.Data.Entity;
using Lib.data.ef;
using Hiwjcn.Core.Domain;
using Hiwjcn.Core.Entity;

namespace Hiwjcn.Bll
{
    public interface ISystemService : IServiceBase<SystemEntity>
    {
        Task<List<SystemEntity>> QueryAll();
    }

    public class SystemService : ServiceBase<SystemEntity>, ISystemService
    {
        private readonly IEFRepository<SystemEntity> _sysRepo;

        public SystemService(
            IEFRepository<SystemEntity> _sysRepo)
        {
            this._sysRepo = _sysRepo;
        }

        public async Task<List<SystemEntity>> QueryAll() =>
            await this._sysRepo.GetListAsync(x => x.IID > 0, count: 5000);
    }
}
