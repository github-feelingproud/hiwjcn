using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Lib.helper;
using Lib.data;
using Lib.core;
using Lib.extension;
using System.ComponentModel.DataAnnotations;

namespace Model
{
    /// <summary>
    /// 实体基类
    /// </summary>
    public abstract class BaseEntity : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(nameof(IID))]
        public virtual int IID { get; set; }

        [Index]
        [Required]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "记录UID必填")]
        public virtual string UID { get; set; }

        public virtual int IsRemove { get; set; }

        public virtual DateTime CreateTime { get; set; }

        public virtual DateTime UpdateTime { get; set; }

        public virtual void Init()
        {
            var now = DateTime.Now;
            this.IID = 0;
            this.UID = Com.GetUUID();
            this.IsRemove = (int)YesOrNoEnum.否;
            this.CreateTime = now;
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
}
