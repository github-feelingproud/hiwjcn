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

namespace Model
{
    [Serializable]
    public abstract class TreeBaseEntity : BaseEntity
    {
        public const int FIRST_LEVEL = 0;
        public const string FIRST_PARENT_UID = "";

        [Required]
        [Range(FIRST_LEVEL, FIRST_LEVEL + 500, ErrorMessage = "层级不在范围之内")]
        public virtual int Level { get; set; } = FIRST_LEVEL;

        [Required]
        [StringLength(100, ErrorMessage = "父级UID长度错误")]
        public virtual string ParentUID { get; set; } = FIRST_PARENT_UID;
    }

    public interface ITreeServiceBase<T> : IServiceBase<T>
        where T : TreeBaseEntity
    {
        Task<List<T>> FindNodeChildrenRecursively_(IQueryable<T> data_source, T first_node,
               string tree_error = "树存在无限递归");
    }

    public abstract class TreeServiceBase<T> : ServiceBase<T>, ITreeServiceBase<T>
        where T : TreeBaseEntity
    {
        public async Task<List<T>> FindNodeChildrenRecursively_(IQueryable<T> data_source, T first_node,
            string tree_error = "树存在无限递归")
        {
            var repeat_check = new List<string>();
            var list = new List<T>();

            async Task FindRecursively(T node)
            {
                if (node == null) { return; }

                repeat_check.AddOnceOrThrow(node.UID, error_msg: tree_error);

                var child_parent_uid = node.UID;
                var child_level = node.Level + 1;
                var children = await data_source.Where(x => x.ParentUID == child_parent_uid && x.Level == child_level).ToListAsync();
                if (ValidateHelper.IsPlumpList(children))
                {
                    foreach (var child in children)
                    {
                        await FindRecursively(child);
                    }
                }

                list.Add(node);
            }

            await FindRecursively(first_node);

            return list;
        }
    }

    [Serializable]
    public abstract class TimeBaseEntity : BaseEntity
    {
        public virtual int TimeYear { get; set; }

        public virtual int TimeMonth { get; set; }

        public virtual int TimeDay { get; set; }

        public virtual int TimeHour { get; set; }

        public override void Init(string flag = null)
        {
            base.Init(flag);
            this.TimeYear = this.CreateTime.Year;
            this.TimeMonth = this.CreateTime.Month;
            this.TimeDay = this.CreateTime.Day;
            this.TimeHour = this.CreateTime.Hour;
        }
    }

    /// <summary>
    /// 实体基类
    /// </summary>
    [Serializable]
    public abstract class BaseEntity : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(nameof(IID))]
        public virtual long IID { get; set; }

        [StringLength(100, MinimumLength = 20, ErrorMessage = "UID必填")]
        [Required]
        [Index(IsUnique = true), Column(nameof(UID))]
        public virtual string UID { get; set; }

        [Index(IsUnique = false)]
        public virtual int IsRemove { get; set; }

        [Index(IsUnique = false)]
        public virtual DateTime CreateTime { get; set; }

        [Index(IsUnique = false)]
        public virtual DateTime UpdateTime { get; set; }

        [Obsolete("用一个uuid来标识此次数据更新，以后ES就可以用这个字段判断数据是否是最新的")]
        [NotMapped]
        public virtual string UpdateFlag { get; set; }

        /// <summary>
        /// 第一次写库初始化
        /// </summary>
        public virtual void Init(string flag = null)
        {
            flag = ConvertHelper.GetString(flag);
            if (flag.ToArray().Any(x => x == ' '))
            {
                throw new Exception("init.flag不可以出现空格");
            }
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
            this.UpdateFlag = Com.GetUUID();
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
