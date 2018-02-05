using Lib.infrastructure.entity;
using Lib.infrastructure.entity.auth;
using Lib.infrastructure.entity.user;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EPC.Core.Entity
{
    [Serializable]
    [Table("tb_page")]
    public class PageEntity : BaseEntity
    {
        [Required]
        public virtual string UserUID { get; set; }

        [Required]
        public virtual string PageTitle { get; set; }

        [Required]
        public virtual string PageDescription { get; set; }

        public virtual string PageMetaJson { get; set; }

        [DataType(DataType.Html)]
        public virtual string PageContent { get; set; }

        [DataType(DataType.Text)]
        public virtual string PageContentMarkdown { get; set; }

        public virtual DateTime? PageStartTime { get; set; }

        public virtual DateTime? PageEndTime { get; set; }

        public virtual int PageStatus { get; set; }

        public virtual int PageType { get; set; }

        public virtual int CommentStatus { get; set; }

        public virtual string PagePassword { get; set; }

        [Required]
        [Index(IsUnique = true)]
        public virtual string PageName { get; set; }

        public virtual int CommentCount { get; set; }

        public virtual int Sort { get; set; }

        [Required]
        public virtual string PageGroup { get; set; } = string.Empty;

        public virtual int PageLanguage { get; set; }
        
        public virtual bool ShowWhen(DateTime time) =>
            (this.PageStartTime == null || this.PageStartTime < time) &&
            (this.PageEndTime == null || this.PageEndTime > time);

        public virtual string DeviceUID { get; set; }

        [Required]
        public virtual string OrgUID { get; set; }

        [NotMapped]
        public virtual DeviceEntity DeviceModel { get; set; }
    }
}
