using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.infrastructure.entity
{
    /// <summary>
    /// 搜索邮箱：
    /// 【终极解决方案】calendar周期性事件，可以高效查询的数据库设计和存储方案
    /// </summary>
    public class CalendarEventTimeEntity : BaseEntity
    {
        public virtual string EventUID { get; set; }

        public virtual DateTime Start { get; set; }

        public virtual DateTime? End { get; set; }

        public virtual int? DailyInterval { get; set; }

        public virtual int? InDayOfWeek { get; set; }
        
        public virtual DateTime? NotInRangeStart { get; set; }

        public virtual DateTime? NotInRangeEnd { get; set; }
    }
}
