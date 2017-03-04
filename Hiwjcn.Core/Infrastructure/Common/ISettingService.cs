using Lib.infrastructure;
using Model.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiwjcn.Core.Infrastructure.Common
{
    public interface ISettingService : IServiceBase<OptionModel>
    {
        /// <summary>
        /// 保存配置对象
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        string SaveOption(OptionModel model);

        /// <summary>
        /// 获取所有配置对象
        /// </summary>
        /// <returns></returns>
        List<OptionModel> GetAllOptions();
    }
}
