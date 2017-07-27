using Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiwjcn.Core.Model.Sys
{
    [Table("sys_upfile")]
    public class UpFileModel : BaseEntity
    {
        [Column("UserID")]
        public virtual string UserID { get; set; }

        [Column("FileName")]
        public virtual string FileName { get; set; }

        [Column("FileSize")]
        public virtual int FileSize { get; set; }

        [Column("FileExt")]
        public virtual string FileExt { get; set; }

        [Column("FilePath")]
        public virtual string FilePath { get; set; }

        [Column("FileUrl")]
        public virtual string FileUrl { get; set; }

        [Column("FileMD5")]
        public virtual string FileMD5 { get; set; }
    }
}
