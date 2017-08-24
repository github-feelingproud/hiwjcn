using Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.cache;
using Lib.core;
using Lib.helper;
using Lib.extension;

namespace Hiwjcn.Core.Model.Sys
{
    public class ReqLogGroupModel
    {
        public virtual string AreaName { get; set; }

        public virtual string ControllerName { get; set; }

        public virtual string ActionName { get; set; }

        public virtual string Url
        {
            get
            {
                var sp = new string[] { this.AreaName, this.ControllerName, this.ActionName }.Where(x => ValidateHelper.IsPlumpString(x));
                return "/".Join_(sp);
            }
        }


        public virtual int Year { get; set; }

        public virtual int Month { get; set; }

        public virtual int Day { get; set; }

        public virtual int Hour { get; set; }


        public virtual double? ReqTime { get; set; }

        public virtual int ReqCount { get; set; }

        private DateTime? _time;
        public virtual DateTime Time
        {
            get
            {
                try
                {
                    if (this._time == null)
                    {
                        this._time = new DateTime(this.Year, this.Month, this.Day);
                    }
                    return this._time.Value;
                }
                catch
                {
                    this._time = DateTime.MinValue;
                    return this._time.Value;
                }
            }
        }
    }

    [Serializable]
    [Table("sys_reqlog")]
    public class ReqLogModel : TimeBaseEntity
    {
        [MaxLength(100)]
        public virtual string ReqID { get; set; }

        [MaxLength(1000)]
        public virtual string ReqRefURL { get; set; }

        [MaxLength(1000)]
        public virtual string ReqURL { get; set; }

        [MaxLength(50)]
        public virtual string AreaName { get; set; }

        [MaxLength(50)]
        public virtual string ControllerName { get; set; }

        [MaxLength(50)]
        public virtual string ActionName { get; set; }

        [MaxLength(50)]
        public virtual string ReqMethod { get; set; }

        [DataType(DataType.Text)]
        public virtual string PostParams { get; set; }

        [MaxLength(1000)]
        public virtual string GetParams { get; set; }

        public virtual double? ReqTime { get; set; }
    }

    public class CacheHitGroupModel
    {
        public virtual string CacheKey { get; set; }

        public virtual int Year { get; set; }

        public virtual int Month { get; set; }

        public virtual int Day { get; set; }

        public virtual int Hour { get; set; }

        public virtual int HitCount { get; set; }

        public virtual int NotHitCount { get; set; }

        public virtual int AllCount { get => this.HitCount + this.NotHitCount; }

        private DateTime? _time;
        public virtual DateTime Time
        {
            get
            {
                try
                {
                    if (this._time == null)
                    {
                        this._time = new DateTime(this.Year, this.Month, this.Day);
                    }
                    return this._time.Value;
                }
                catch
                {
                    this._time = DateTime.MinValue;
                    return this._time.Value;
                }
            }
        }

        public virtual string Per
        {
            get
            {
                if (this.AllCount <= 0)
                {
                    return string.Empty;
                }
                return $"{(((double)this.HitCount / (double)this.AllCount) * 100).ToString("0.00")}%";
            }
        }
    }

    /// <summary>
    /// 命中缓存的概率统计
    /// </summary>
    [Serializable]
    [Table("sys_cachehitlog")]
    public class CacheHitLog : TimeBaseEntity
    {
        /// <summary>
        /// 没有无参构造函数autofac会报错
        /// </summary>
        public CacheHitLog() { }

        public CacheHitLog(string cacheKey, CacheHitStatusEnum hit)
        {
            this.CacheKey = cacheKey;
            this.HitValue = (int)hit;
            if (hit == CacheHitStatusEnum.Hit)
            {
                this.Hit = (int)YesOrNoEnum.是;
            }
            else
            {
                this.NotHit = (int)YesOrNoEnum.是;
            }
        }

        [Required]
        [MaxLength(3000)]
        public virtual string CacheKey { get; set; }

        public virtual int HitValue { get; set; }

        public virtual int Hit { get; set; }

        public virtual int NotHit { get; set; }
    }
}
