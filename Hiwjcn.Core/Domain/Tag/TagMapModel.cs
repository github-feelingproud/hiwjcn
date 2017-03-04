using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebLogic.Model;
using Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebLogic.Model.Tag
{
    [Table("wp_tag_map")]
    public class TagMapModel : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("map_id")]
        public virtual int MapID { get; set; }

        [Column("map_key")]
        public virtual string MapKey { get; set; }

        [Column("tag_name")]
        public virtual string TagName { get; set; }

        [Column("map_type")]
        public virtual string MapType { get; set; }
    }
}
