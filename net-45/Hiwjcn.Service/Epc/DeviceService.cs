using EPC.Core;
using EPC.Core.Entity;
using Hiwjcn.Service.Epc.InputsType;
using Lib.core;
using Lib.data.ef;
using Lib.extension;
using Lib.helper;
using Lib.infrastructure;
using Lib.mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Hiwjcn.Core;

namespace Hiwjcn.Service.Epc
{
    public interface IDeviceService : IServiceBase<DeviceEntity>
    {
        Task<DeviceEntity> GetDeviceByUID(string org_uid, string uid);

        Task<_<string>> AddDevice(DeviceEntity model);

        Task<_<string>> UpdateDevice(DeviceEntity model);

        Task<PagerData<DeviceEntity>> QueryDevice(string org_uid,
            string q = null,
            int page = 1, int pagesize = 10);

        Task<List<DeviceEntity>> QueryAll(string org_uid);

        Task<_<string>> DeleteDevice(string org_uid, string uid);

        Task<List<DeviceEntity>> _LoadDeviceExtraData(List<DeviceEntity> data);
    }

    public class DeviceService : ServiceBase<DeviceEntity>, IDeviceService
    {
        private readonly IEpcRepository<DeviceEntity> _deviceRepo;
        private readonly IEpcRepository<DeviceParameterEntity> _deviceParamRepo;

        public DeviceService(
            IEpcRepository<DeviceEntity> _deviceRepo,
            IEpcRepository<DeviceParameterEntity> _deviceParamRepo)
        {
            this._deviceRepo = _deviceRepo;
            this._deviceParamRepo = _deviceParamRepo;
        }

        private void PrepareParams(ref List<DeviceParameterEntity> list, string device_uid)
        {
            if (list == null) { throw new ArgumentNullException(); }
            if (!ValidateHelper.IsPlumpString(device_uid)) { throw new MsgException("设备UID丢失"); }
            foreach (var m in list)
            {
                m.Init("p");
                m.DeviceUID = device_uid;
                if (!m.IsValid(out var msg))
                {
                    throw new MsgException(msg);
                }
                InputExpressionValidateable input = null;
                switch (m.InputType)
                {
                    case (int)DeviceParameterTypeEnum.字符:
                        input = m.Rule?.JsonToEntity<StringInputExpression>(throwIfException: false);
                        break;
                    case (int)DeviceParameterTypeEnum.布尔:
                        input = m.Rule?.JsonToEntity<BoolInputExpression>(throwIfException: false);
                        break;
                    case (int)DeviceParameterTypeEnum.数值:
                        input = m.Rule?.JsonToEntity<NumberInputExpression>(throwIfException: false);
                        break;
                    default:
                        break;
                }
                if (input == null)
                {
                    throw new MsgException("参数类型错误");
                }
                if (!input.PrepareAndValid(out var err))
                {
                    throw new MsgException(err);
                }
                m.Rule = input.ToJson();
            }
        }

        public virtual async Task<_<string>> AddDevice(DeviceEntity model)
        {
            var res = new _<string>();
            try
            {
                model.Init("d");
                if (!model.IsValid(out var msg))
                {
                    throw new MsgException(msg);
                }

                var list = ConvertHelper.NotNullList(model.ParamsList);
                this.PrepareParams(ref list, model.UID);

                await this._deviceRepo.PrepareSessionAsync(async db =>
                {
                    db.Set<DeviceEntity>().Add(model);

                    if (ValidateHelper.IsPlumpList(list))
                    {
                        db.Set<DeviceParameterEntity>().AddRange(list);
                    }

                    await db.SaveChangesAsync();
                });

                res.SetSuccessData(string.Empty);
                return res;
            }
            catch (MsgException e)
            {
                res.SetErrorMsg(e.Message);
                return res;
            }
        }

        public virtual async Task<_<string>> UpdateDevice(DeviceEntity model)
        {
            var res = new _<string>();
            try
            {
                await this._deviceRepo.PrepareSessionAsync(async db =>
                {
                    var device_set = db.Set<DeviceEntity>();
                    var param_set = db.Set<DeviceParameterEntity>();

                    var entity = await device_set.Where(x => x.UID == model.UID).FirstOrDefaultAsync();
                    if (entity == null) { throw new MsgException("数据不存在"); }

                    entity.Name = model.Name;
                    entity.Description = model.Description;
                    entity.ParamsList = model.ParamsList;
                    entity.Update();

                    if (!entity.IsValid(out var msg))
                    {
                        throw new MsgException(msg);
                    }

                    var list = ConvertHelper.NotNullList(model.ParamsList);
                    this.PrepareParams(ref list, entity.UID);

                    if (ValidateHelper.IsPlumpList(entity.ParamsList))
                    {
                        param_set.AddRange(list);
                    }

                    var old_param_uids = await param_set.Where(x => x.DeviceUID == entity.UID).Select(x => x.UID).ToListAsync();
                    if (ValidateHelper.IsPlumpList(old_param_uids))
                    {
                        param_set.RemoveRange(param_set.Where(x => old_param_uids.Contains(x.UID)));
                    }

                    await db.SaveChangesAsync();
                });

                res.SetSuccessData(string.Empty);
                return res;
            }
            catch (MsgException e)
            {
                res.SetErrorMsg(e.Message);
                return res;
            }
        }

        public virtual async Task<PagerData<DeviceEntity>> QueryDevice(string org_uid,
            string q = null,
            int page = 1, int pagesize = 10)
        {
            return await this._deviceRepo.PrepareIQueryableAsync(async query =>
            {
                query = query.Where(x => x.OrgUID == org_uid);
                if (ValidateHelper.IsPlumpString(q))
                {
                    query = query.Where(x => x.Name.Contains(q));
                }

                var data = await query.ToPagedListAsync(page, pagesize, x => x.IID);

                return data;
            });
        }

        public async Task<_<string>> DeleteDevice(string org_uid, string uid)
        {
            var res = new _<string>();

            if (await this._deviceRepo.DeleteWhereAsync(x => x.OrgUID == org_uid && x.UID == uid) > 0)
            {
                await this._deviceParamRepo.DeleteWhereAsync(x => x.DeviceUID == uid);
            }

            res.SetSuccessData(string.Empty);
            return res;
        }

        public async Task<DeviceEntity> GetDeviceByUID(string org_uid, string uid)
        {
            var model = await this._deviceRepo.GetFirstAsync(x => x.UID == uid);
            if (model?.OrgUID != org_uid) { throw new DataNotExistException("数据不存在"); }

            return model;
        }

        public async Task<List<DeviceEntity>> QueryAll(string org_uid)
        {
            var data = await this._deviceRepo.GetListAsync(x => x.OrgUID == org_uid, count: 1000);
            data = data.OrderByDescending(x => x.IID).ToList();
            return data;
        }

        public async Task<List<DeviceEntity>> _LoadDeviceExtraData(List<DeviceEntity> data)
        {
            if (!ValidateHelper.IsPlumpList(data))
            {
                return data;
            }

            var uids = data.Select(x => x.UID);
            var list = await this._deviceParamRepo.GetListAsync(x => uids.Contains(x.DeviceUID));

            foreach (var m in data)
            {
                m.ParamsList = list.Where(x => x.DeviceUID == m.UID).ToList();
            }

            return data;
        }
    }
}
