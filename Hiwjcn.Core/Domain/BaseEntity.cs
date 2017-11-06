using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Lib.helper;
using Lib.data;
using Lib.core;
using Lib.extension;
using Lib.infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Model
{

    [Serializable]
    public abstract class TreeBaseEntity : Lib.infrastructure.entity.TreeBaseEntity { }

    [Serializable]
    public abstract class TimeBaseEntity : Lib.infrastructure.entity.TimeBaseEntity { }

    /// <summary>
    /// 实体基类
    /// </summary>
    [Serializable]
    public abstract class BaseEntity : Lib.infrastructure.entity.BaseEntity { }

}
