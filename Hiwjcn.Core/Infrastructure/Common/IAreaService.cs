using Lib.infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebLogic.Model.Sys;

namespace Hiwjcn.Core.Infrastructure.Common
{
    public interface IAreaService : IServiceBase<AreaModel>
    {
        List<AreaModel> GetAreas(int level, string parent, int max_count = 500);
    }
}
