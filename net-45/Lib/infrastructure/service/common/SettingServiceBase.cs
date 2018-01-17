using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;
using Lib.infrastructure.entity.common;
using Lib.data;
using Lib.mvc;
using Lib.helper;
using Lib.extension;
using System.Data.Entity;
using Lib.core;
using Lib.infrastructure.extension;
using Lib.data.ef;

namespace Lib.infrastructure.service.common
{
    public class SettingServiceBase<SettingBase>
        where SettingBase : SettingEntityBase
    {
        protected readonly IEFRepository<SettingBase> _settingRepo;

        public SettingServiceBase(IEFRepository<SettingBase> _settingRepo)
        {
            this._settingRepo = _settingRepo;
        }

        public virtual async Task<List<SettingBase>> GetSettingsBy(string user_uid, string org_uid, string group_key)
        {
            if (!ValidateHelper.IsAnyPlumpString(user_uid, org_uid, group_key)) { throw new Exception("参数错误"); }
            return await this._settingRepo.PrepareIQueryableAsync(async query =>
            {
                if (ValidateHelper.IsPlumpString(user_uid))
                {
                    query = query.Where(x => x.UserUID == user_uid);
                }
                if (ValidateHelper.IsPlumpString(org_uid))
                {
                    query = query.Where(x => x.OrgUID == org_uid);
                }
                if (ValidateHelper.IsPlumpString(group_key))
                {
                    query = query.Where(x => x.GroupKey == group_key);
                }
                var data = await query.Take(5000).ToListAsync();
                if (data.Count > 1000)
                {
                    new Exception($"拉取设置数量过多，参数：user:{user_uid},org:{org_uid},group:{group_key}").AddErrorLog();
                }
                return data;
            });
        }

        public virtual async Task<_<SettingBase>> SaveSettings(SettingBase model) =>
            await this._settingRepo.AddEntity_(model, "st");

    }
}
