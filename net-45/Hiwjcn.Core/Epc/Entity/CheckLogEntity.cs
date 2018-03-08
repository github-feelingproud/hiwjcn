using Hiwjcn.Core.Domain.User;
using Lib.infrastructure.entity;
using Lib.infrastructure.entity.auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPC.Core.Entity
{
    /// <summary>
    /// 设备点检信息
    /// </summary>
    [Serializable]
    [Table("tb_check_log")]
    public class CheckLogEntity : TimeEntityBase, IEpcDBTable
    {
        [Required(ErrorMessage = "组织信息丢失")]
        public virtual string OrgUID { get; set; }

        [Required(ErrorMessage = "用户信息丢失")]
        public virtual string UserUID { get; set; }

        [Required(ErrorMessage = "设备信息丢失")]
        public virtual string DeviceUID { get; set; }

        public virtual int StatusOK { get; set; }

        [DataType(DataType.Text)]
        public virtual string TipsJson { get; set; }

        [NotMapped]
        public virtual List<string> Tips { get; set; }

        [NotMapped]
        public virtual UserEntity UserModel { get; set; }

        [NotMapped]
        public virtual DeviceEntity DeviceModel { get; set; }
    }

    /// <summary>
    /// 设备参数点检信息
    /// </summary>
    [Serializable]
    [Table("tb_check_log_item")]
    public class CheckLogItemEntity : TimeEntityBase, IEpcDBTable
    {
        [Required(ErrorMessage = "点检记录丢失")]
        public virtual string CheckUID { get; set; }

        [Required(ErrorMessage = "设备信息丢失")]
        public virtual string DeviceUID { get; set; }

        [Required(ErrorMessage = "设备参数丢失")]
        public virtual string ParameterUID { get; set; }

        [Required(ErrorMessage = "设备参数名丢失")]
        public virtual string ParameterName { get; set; }

        [Required(ErrorMessage = "设备参数规则丢失")]
        public virtual string Rule { get; set; }

        [Required(ErrorMessage = "点检数据丢失")]
        public virtual string InputDataJson { get; set; }

        public virtual int StatusOK { get; set; }

        public virtual int InputType { get; set; }

        #region 表格维度

        [NotMapped]
        public virtual double? NormalDouble { get; set; }

        [NotMapped]
        public virtual string NormalString { get; set; }

        [NotMapped]
        public virtual int? NormalBool { get; set; }

        [NotMapped]
        [Obsolete("多个值不保存json，保存多个这样的记录")]
        public virtual string NormalSelectStringJson { get; set; }

        #endregion

        [NotMapped]
        public virtual List<string> Tips { get; set; }
    }
}
