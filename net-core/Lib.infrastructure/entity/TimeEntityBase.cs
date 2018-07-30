using System;

namespace Lib.infrastructure.entity
{
    [Serializable]
    public abstract class TimeEntityBase : BaseEntity
    {
        public virtual int TimeYear { get; set; }

        public virtual int TimeMonth { get; set; }

        public virtual int TimeDay { get; set; }

        public virtual int TimeHour { get; set; }

        public override void Init(string flag = null)
        {
            base.Init(flag);
            this.TimeYear = this.CreateTime.Year;
            this.TimeMonth = this.CreateTime.Month;
            this.TimeDay = this.CreateTime.Day;
            this.TimeHour = this.CreateTime.Hour;
        }
    }
}
