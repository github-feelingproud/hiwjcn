using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.infrastructure.entity
{
    [Serializable]
    public class CatalogEntityBase : TreeEntityBase
    {
        [Required]
        public virtual string CatalogName { get; set; }

        public virtual string CatalogDescription { get; set; }
    }
}
