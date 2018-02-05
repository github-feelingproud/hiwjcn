using Lib.infrastructure.entity;
using Lib.infrastructure.entity.auth;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPC.Core.Entity
{
    [Serializable]
    [Table("tb_calendar_event")]
    public class CalendarEventEntity : BaseEntity
    {
        public virtual DateTime DateStart { get; set; }

        public virtual DateTime? DateEnd { get; set; }

        public virtual int Priority { get; set; }

        public virtual string RRule { get; set; }

        [Required]
        public virtual string Summary { get; set; }

        public virtual string Content { get; set; }

        public virtual string Location { get; set; }

        [Required]
        public virtual string UserUID { get; set; }

        [Required]
        public virtual string OrgUID { get; set; }

        public virtual int HasRule { get; set; }

        public virtual string Color { get; set; }

        /// <summary>
        /// 隔几天触发
        /// </summary>
        [NotMapped]
        public virtual int? ByDay { get; set; }
    }

    [Serializable]
    [Table("tb_event_device")]
    public class EventDeviceEntity : BaseEntity
    {
        public virtual string EventUID { get; set; }

        public virtual string DeviceUID { get; set; }
    }
}
