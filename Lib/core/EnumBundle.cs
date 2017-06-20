using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.core
{
    /// <summary>
    /// 是否
    /// </summary>
    public enum YesOrNoEnum : int
    {
        否 = 0,
        是 = 1
    }

    /// <summary>
    /// 男女未知
    /// </summary>
    public enum SexEnum : int
    {
        男 = 1,
        女 = 0,
        未知 = -1
    }

    /// <summary>
    /// 增删改查
    /// </summary>
    public enum CrudEnum : int
    {
        增 = 1 << 0,
        删 = 1 << 1,
        改 = 1 << 2,
        查 = 1 << 3
    }

}
