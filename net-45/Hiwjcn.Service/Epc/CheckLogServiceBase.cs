using EPC.Core;
using EPC.Core.Entity;
using EPC.Core.Model;
using Hiwjcn.Service.Epc.InputsType;
using Hiwjcn.Core.Data;
using Hiwjcn.Core.Domain.User;
using Lib.core;
using Lib.data.ef;
using Lib.extension;
using Lib.helper;
using Lib.infrastructure;
using Lib.infrastructure.extension;
using Lib.mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Hiwjcn.Service.Epc
{
    public abstract class CheckLogServiceBase : ServiceBase<CheckLogEntity>
    {
        private readonly IEpcRepository<CheckLogEntity> _logRepo;
        private readonly IEpcRepository<CheckLogItemEntity> _logItemRepo;
        private readonly IEpcRepository<DeviceParameterEntity> _paramRepo;
        private readonly IEpcRepository<DeviceEntity> _deviceRepo;
        private readonly IMSRepository<UserEntity> _userRepo;

        public CheckLogServiceBase(
            IEpcRepository<CheckLogEntity> _logRepo,
            IEpcRepository<CheckLogItemEntity> _logItemRepo,
            IEpcRepository<DeviceParameterEntity> _paramRepo,
            IEpcRepository<DeviceEntity> _deviceRepo,
            IMSRepository<UserEntity> _userRepo)
        {
            this._logRepo = _logRepo;
            this._logItemRepo = _logItemRepo;
            this._paramRepo = _paramRepo;
            this._deviceRepo = _deviceRepo;
            this._userRepo = _userRepo;
        }
    }
}
