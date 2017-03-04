using Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiwjcn.Core.Model.Sys
{
    public class ReqLogGroupModel
    {
        public virtual string AreaName { get; set; }

        public virtual string ControllerName { get; set; }

        public virtual string ActionName { get; set; }

        public virtual double? ReqTime { get; set; }

        public virtual int ReqCount { get; set; }
    }

    [Table("wp_reqlog")]
    public class ReqLogModel : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("uid")]
        public virtual long UID { get; set; }

        [Column("req_id")]
        public virtual string ReqID { get; set; }

        [Column("req_ref_url")]
        public virtual string ReqRefURL { get; set; }

        [Column("req_url")]
        public virtual string ReqURL { get; set; }

        [Column("area_name")]
        public virtual string AreaName { get; set; }

        [Column("controller_name")]
        public virtual string ControllerName { get; set; }

        [Column("action_name")]
        public virtual string ActionName { get; set; }

        [Column("req_method")]
        public virtual string ReqMethod { get; set; }

        [Column("post_params")]
        public virtual string PostParams { get; set; }

        [Column("get_params")]
        public virtual string GetParams { get; set; }

        [Column("req_time")]
        public virtual double? ReqTime { get; set; }

        [Column("update_time")]
        public virtual DateTime UpdateTime { get; set; }
    }
}
