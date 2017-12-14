using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.task
{
    public class ScheduleJobModel
    {

        public virtual string JobName { get; set; }

        public virtual string JobGroup { get; set; }

        public virtual string JobStatus { get; set; }

        public virtual string TriggerName { get; set; }

        public virtual string TriggerGroup { get; set; }

        public virtual DateTime StartTime { get; set; }

        public virtual DateTime? PreTriggerTime { get; set; }

        public virtual DateTime? NextTriggerTime { get; set; }

        public virtual bool IsRunning { get; set; }

    }
}
