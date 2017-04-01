using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.data
{
    /// <summary>
    /// 标记是数据表（不要使用类，因为单继承，有时候无法标记）
    /// </summary>
    public interface IDBTable
    {
        //
    }

    public abstract class DBTable : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long IID { get; set; }
        public virtual string UID { get; set; }
    }

}
