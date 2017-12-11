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

namespace Lib.infrastructure.entity
{
    [Serializable]
    public enum PageStatusEnum : int
    {
        草稿 = 0,
        发布 = 1
    }

    [Serializable]
    public enum PageTypeEnum : int
    {
        //
    }

    /// <summary>
    /// /page/show/name_123
    /// /page/show/id_123
    /// /page/group/news
    /// </summary>
    [Serializable]
    public class PageEntityBase : BaseEntity
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
    }
}
