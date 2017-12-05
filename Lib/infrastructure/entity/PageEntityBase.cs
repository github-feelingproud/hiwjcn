using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc.user;
using Lib.extension;
using Lib.helper;

namespace Lib.infrastructure.entity
{
    [Serializable]
    public class PageEntityBase : BaseEntity
    {
        public virtual string UserUID { get; set; }

        public virtual string PostTitle { get; set; }

        public virtual string PostDescription { get; set; }

        public virtual string PostMetaJson { get; set; }

        public virtual string PostContent { get; set; }

        public virtual string PostContentMarkdown { get; set; }

        public virtual DateTime? PostStartTime { get; set; }

        public virtual DateTime? PostEndTime { get; set; }

        public virtual int PostStatus { get; set; }

        public virtual int CommentStatus { get; set; }

        public virtual string PostPassword { get; set; }

        public virtual string PostName { get; set; }

        public virtual int CommentCount { get; set; }

        public virtual int Sort { get; set; }

        public virtual string PostGroup { get; set; }

        public virtual int PageLanguage { get; set; }
    }
}
