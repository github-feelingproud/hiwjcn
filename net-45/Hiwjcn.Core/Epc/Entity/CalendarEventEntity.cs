using Lib.infrastructure.entity;
using Lib.infrastructure.entity.auth;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPC.Core.Entity
{
    [Serializable]
    [Table("tb_calendar_event")]
    public class CalendarEventEntity : BaseEntity, IEpcDBTable
    {
        public virtual DateTime DateStart { get; set; }

        /// <summary>
        /// 字段定义可为空，其实不可以为空
        /// </summary>
        public virtual DateTime? DateEnd { get; set; }

        public virtual int Priority { get; set; }

        [Required(ErrorMessage = "规则不能为空")]
        public virtual string RRule { get; set; }

        [Required(ErrorMessage = "标题不能为空")]
        public virtual string Summary { get; set; }

        public virtual string Content { get; set; }

        public virtual string Location { get; set; }

        [Required(ErrorMessage = "设备信息不能为空")]
        public virtual string DeviceUID { get; set; }

        [Required(ErrorMessage = "用户ID为空")]
        public virtual string UserUID { get; set; }

        [Required(ErrorMessage = "组织ID为空")]
        public virtual string OrgUID { get; set; }

        public virtual int HasRule { get; set; }

        public virtual string Color { get; set; }

        /// <summary>
        /// 隔几天触发
        /// </summary>
        [NotMapped]
        public virtual int? ByDay { get; set; }
    }
}
