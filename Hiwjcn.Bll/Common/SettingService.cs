using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;
using System.Threading.Tasks;
using Lib;
using Model;
using Model.Sys;
using Lib.core;
using Lib.helper;
using Lib.http;
using Dal.Sys;
using Hiwjcn.Core.Infrastructure.Common;

namespace Bll.Sys
{
    /// <summary>
    /// 系统配置
    /// </summary>
    public class SettingService : ServiceBase<OptionModel>, ISettingService
    {
        private OptionDal _OptionDal { get; set; }

        public SettingService()
        {
            this._OptionDal = new OptionDal();
        }

        public override string CheckModel(OptionModel model)
        {
            if (model == null) { return "配置对象为空"; }
            if (!ValidateHelper.IsPlumpString(model.Key))
            {
                return "配置key为空";
            }
            return string.Empty;
        }

        /// <summary>
        /// 保存配置对象
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string SaveOption(OptionModel model)
        {
            string errinfo = CheckModel(model);
            if (ValidateHelper.IsPlumpString(errinfo)) { return errinfo; }
            var list = _OptionDal.QueryList<object>(where: x => x.Key == model.Key);
            if (ValidateHelper.IsPlumpList(list))
            {
                _OptionDal.Delete(list.ToArray());
            }
            if (!ValidateHelper.IsPlumpString(model.Value))
            {
                return SUCCESS;
            }
            return _OptionDal.Add(model) > 0 ? SUCCESS : "修改失败";
        }

        /// <summary>
        /// 获取所有配置对象
        /// </summary>
        /// <returns></returns>
        public List<OptionModel> GetAllOptions()
        {
            string cacheid = "OptionBll.GetAllOptions";
            return Cache(cacheid, () => _OptionDal.GetList(null));
        }
    }
}
