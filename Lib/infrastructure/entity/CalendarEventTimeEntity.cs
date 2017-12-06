using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.infrastructure.entity
{
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
