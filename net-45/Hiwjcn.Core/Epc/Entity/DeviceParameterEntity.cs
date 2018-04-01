using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;
using Lib.extension;
using Lib.helper;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPC.Core.Entity
{
    [Serializable]
    public enum DeviceParameterTypeEnum : int
    {
        //比如输入温度，温度不在合理值提示异常信息
        数值 = 1,
        //场景还没想到，先加上再说
        字符 = 2,
        //比如开关是否打开，如果不是就提示信息
        布尔 = 3,
    }

    [Serializable]
    [Table("tb_device_parameter")]
    public class DeviceParameterEntity : BaseEntity, IEpcDBTable
    {
        /// <summary>
        /// 设备UID
        /// </summary>
        [Required(ErrorMessage = "设备ID丢失")]
        public virtual string DeviceUID { get; set; }

        /// <summary>
        /// 参数名称，比如水温，电压
        /// </summary>
        [Required(ErrorMessage = "参数名不能为空")]
        public virtual string ParameterName { get; set; }

        /// <summary>
        /// 参数的输入规则，4种
        /// </summary>
        [Required(ErrorMessage = "参数规则不能为空")]
        public virtual string Rule { get; set; }

        /// <summary>
        /// 输入规则的类型，参照上面的枚举
        /// </summary>
        public virtual int InputType { get; set; }
    }
}
