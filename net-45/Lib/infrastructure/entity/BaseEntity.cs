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
    /// <summary>
    /// 实体基类
    /// </summary>
    [Serializable]
    public abstract class BaseEntity : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(nameof(IID)), Index(IsUnique = true)]
        public virtual long IID { get; set; }

        [StringLength(500, MinimumLength = 1, ErrorMessage = "UID必填")]
        [Column(nameof(UID)), Index(IsUnique = true), Required]
        public virtual string UID { get; set; }

        public virtual int IsRemove { get; set; }

        public virtual DateTime CreateTime { get; set; }

        public virtual DateTime UpdateTime { get; set; }

        /// <summary>
        /// 状态正常
        /// </summary>
        /// <returns></returns>
        public virtual bool OK() => this.IsRemove <= 0;

        /// <summary>
        /// 第一次写库初始化
        /// </summary>
        public virtual void Init(string flag = null)
        {
            var prefix = string.Empty;
            if (ValidateHelper.IsPlumpString(flag))
            {
                prefix = $"{flag}-";
            }

            var now = DateTime.Now;

            this.IID = default(long);
            this.UID = prefix + Com.GetUUID();
            this.IsRemove = (int)YesOrNoEnum.否;
            this.CreateTime = now;

            this.Update();
        }

        /// <summary>
        /// 更新时间等信息
        /// </summary>
        public virtual void Update()
        {
            var now = DateTime.Now;
            this.UpdateTime = now;
        }

        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToJson();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BaseEntity);
        }

        private static bool IsTransient(BaseEntity obj)
        {
            return obj != null && Equals(obj.IID, default(int));
        }

        private Type GetUnproxiedType()
        {
            return GetType();
        }

        public virtual bool Equals(BaseEntity other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (!IsTransient(this) &&
                !IsTransient(other) &&
                Equals(IID, other.IID))
            {
                var otherType = other.GetUnproxiedType();
                var thisType = GetUnproxiedType();
                return thisType.IsAssignableFrom(otherType) ||
                        otherType.IsAssignableFrom(thisType);
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (Equals(IID, default(int)))
                return base.GetHashCode();
            return IID.GetHashCode();
        }

        public static bool operator ==(BaseEntity x, BaseEntity y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(BaseEntity x, BaseEntity y)
        {
            return !(x == y);
        }
    }

    /// <summary>
    /// 搜索邮箱有资料
    /// 【EF使用乐观锁，并发token&行版本】并发标记
    /// https://docs.microsoft.com/zh-cn/ef/core/modeling/concurrency
    /// </summary>
    [Obsolete("不要使用")]
    internal abstract class RowVersionTest : BaseEntity
    {
        /// <summary>
        /// where iid=@id and name=@name
        /// </summary>
        [ConcurrencyCheck]
        public virtual string Name { get; set; }

        /// <summary>
        /// update xx set rowversion=@newrowversion where iid=@id and name=@name
        /// 
        /// sqlserver 通常使用byte[]
        /// 但是不一定要使用byte[]，使用自动生成字段+ConcurrencyCheck也行
        /// </summary>
        [Timestamp]
        public virtual byte[] RowVersion { get; set; }
    }

    /// <summary>
    /// 搜索邮箱：
    /// 使用rrule
    /// 【终极解决方案】calendar周期性事件，可以高效查询的数据库设计和存储方案
    /// </summary>
    [Serializable]
    [Obsolete("使用rrule")]
    public class CalendarEventTimeEntity : BaseEntity
    {
        public virtual string RRule { get; set; }

        public virtual DateTime Start { get; set; }

        public virtual DateTime? End { get; set; }
    }

}
