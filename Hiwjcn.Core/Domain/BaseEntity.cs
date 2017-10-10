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

    /// <summary>
    /// 树结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITreeServiceBase<T> : IServiceBase<T>
        where T : TreeBaseEntity
    {
        Task<List<T>> FindNodeChildrenRecursively_(IQueryable<T> data_source, T first_node,
               string tree_error = null);

        Task<(bool success, List<T> node_path)> CheckNodeIfCanFindRoot(IQueryable<T> data_source, T first_node);

        Task<List<T>> FindTreeBadNodes(IQueryable<T> data_source);
    }

    /// <summary>
    /// 树结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
                        //递归
                        await FindRecursively(child);
                    }
                }

                list.Add(node);
            }

            await FindRecursively(first_node);

            return list;
        }

        public async Task<(bool success, List<T> node_path)> CheckNodeIfCanFindRoot(IQueryable<T> data_source, T first_node)
        {
            var repeat_check = new List<string>();
            var current_uid = first_node.UID;

            var top_level = default(int?);
            var top_parent = default(string);

            var node = default(T);

            var node_path = new List<T>();

            while (true)
            {
                node = await data_source.Where(x => x.UID == current_uid).FirstOrDefaultAsync();
                if (node == null) { break; }

                repeat_check.AddOnceOrThrow(node.UID, error_msg: "存在无限循环");

                {
                    //设置检查值
                    top_level = node.Level - 1;
                    top_parent = node.ParentUID;
                }

                current_uid = node.ParentUID;
                node_path.Add(node);
            }
            var success = top_level == TreeBaseEntity.FIRST_LEVEL && top_parent == TreeBaseEntity.FIRST_PARENT_UID;
            return (success, node_path);
        }

        public async Task<List<T>> FindTreeBadNodes(IQueryable<T> data_source)
        {
            var list = await data_source.ToListAsync();
            var error_list = new List<string>();

            foreach (var node in list.OrderByDescending(x => x.Level))
            {
                if (error_list.Contains(node.UID))
                {
                    //防止中间节点被重复计算
                    //node1->node2->node3->node4->node5
                    //计算了node1到node5为错误节点之后将跳过1,2,3,4的检查
                    continue;
                }
                var check = await this.CheckNodeIfCanFindRoot(list.AsQueryable(), node);
                if (!check.success)
                {
                    error_list.AddRange(check.node_path.Select(x => x.UID));
                }
            }

            return list.Where(x => error_list.Distinct().Contains(x.UID)).ToList();
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
