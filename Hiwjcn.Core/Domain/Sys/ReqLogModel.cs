using Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiwjcn.Core.Model.Sys
{
    public class ReqLogGroupModel
    {
        public virtual string AreaName { get; set; }

        public virtual string ControllerName { get; set; }

        public virtual string ActionName { get; set; }

        public virtual double? ReqTime { get; set; }

        public virtual int ReqCount { get; set; }
    }

    [Serializable]
    [Table("sys_reqlog")]
    public class ReqLogModel : TimeBaseEntity
    {
        public virtual string ReqID { get; set; }

        public virtual string ReqRefURL { get; set; }

        public virtual string ReqURL { get; set; }

        public virtual string AreaName { get; set; }

        public virtual string ControllerName { get; set; }

        public virtual string ActionName { get; set; }

        public virtual string ReqMethod { get; set; }

        public virtual string PostParams { get; set; }

        public virtual string GetParams { get; set; }

        public virtual double? ReqTime { get; set; }
    }

    /// <summary>
    /// 命中缓存的概率统计
    /// </summary>
    [Serializable]
    [Table("sys_cachehitlog")]
    public class CacheHitLog : TimeBaseEntity
    {
        [Required]
        [MaxLength(3000)]
        public virtual string CacheKey { get; set; }

        public virtual int Hit { get; set; }
    }
}
