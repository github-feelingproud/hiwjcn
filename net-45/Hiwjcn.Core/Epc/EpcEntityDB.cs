using EPC.Core.Entity;
using Lib.data;
using Lib.data.ef;
using MySql.Data.Entity;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;

namespace EPC.Core
{
    /// <summary>
    /// EF容器上下文
    /// EF做映射的时候也可以数据库是int，代码里是string，
    /// 用attribute mapping测试成功，用fluent测试失败
    /// </summary>
    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class EpcEntityDB : BaseEFContext
    {
        public EpcEntityDB() : base(
            ConfigurationManager.ConnectionStrings["epc"]?.ConnectionString ??
            throw new Exception("请配置epc数据库链接字符串"))
        {
            //
        }

        #region model对应的实体

        public virtual DbSet<CalendarEventEntity> CalendarEventEntity { get; set; }

        public virtual DbSet<IssueEntity> IssueEntity { get; set; }
        public virtual DbSet<IssueOperationLogEntity> IssueOperationLogEntity { get; set; }
        public virtual DbSet<DeviceEntity> DeviceEntity { get; set; }
        public virtual DbSet<DeviceParameterEntity> DeviceParameterEntity { get; set; }
        public virtual DbSet<CheckLogEntity> CheckLogEntity { get; set; }
        public virtual DbSet<CheckLogItemEntity> CheckLogItemEntity { get; set; }

        public virtual DbSet<PageEntity> PageEntity { get; set; }

        #endregion
    }
}
