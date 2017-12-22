#define use_mysql

using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Core.Domain.Sys;
using Hiwjcn.Core.Domain.User;
using Lib.data.ef;
using MySql.Data.Entity;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;

namespace Hiwjcn.Dal
{
    /// <summary>
    /// sqlserver数据库
    /// </summary>
    public class SqlServerConfiguration : DbConfiguration
    {
        public SqlServerConfiguration()
        {
            this.SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
            this.SetDefaultConnectionFactory(new LocalDbConnectionFactory("mssqllocaldb"));
        }
    }

    /// <summary>
    /// EF容器上下文
    /// EF做映射的时候也可以数据库是int，代码里是string，
    /// 用attribute mapping测试成功，用fluent测试失败
    /// </summary>
#if use_mysql_
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
#else
    [DbConfigurationType(typeof(SqlServerConfiguration))]
#endif
    public class EntityDB : BaseEFContext
    {
        public EntityDB() : base(
            ConfigurationManager.ConnectionStrings["db"]?.ConnectionString ??
            throw new Exception("请配置db数据库链接字符串"))
        {
            //
        }

        #region model对应的实体

        public virtual DbSet<AuthClient> AuthClient { get; set; }
        public virtual DbSet<AuthScope> AuthScope { get; set; }
        public virtual DbSet<AuthToken> AuthToken { get; set; }
        public virtual DbSet<AuthTokenScope> AuthTokenScope { get; set; }
        public virtual DbSet<AuthCode> AuthCode { get; set; }

        public virtual DbSet<AuthClientCheckLog> AuthClientCheckLog { get; set; }
        public virtual DbSet<AuthClientUseage> AuthClientUseage { get; set; }

        public virtual DbSet<UserAvatarEntity> UserAvatar { get; set; }
        public virtual DbSet<UserEntity> UserEntity { get; set; }
        public virtual DbSet<UserOneTimeCodeEntity> UserOneTimeCode { get; set; }

        public virtual DbSet<LoginErrorLogEntity> LoginErrorLogEntity { get; set; }

        public virtual DbSet<RoleEntity> RoleEntity { get; set; }
        public virtual DbSet<RolePermissionEntity> RolePermissionEntity { get; set; }
        public virtual DbSet<UserRoleEntity> UserRoleEntity { get; set; }

        public virtual DbSet<PermissionEntity> PermissionEntity { get; set; }

        public virtual DbSet<DepartmentEntity> DepartmentEntity { get; set; }
        public virtual DbSet<UserDepartmentEntity> UserDepartmentEntity { get; set; }
        public virtual DbSet<DepartmentRoleEntity> DepartmenetRoleEntity { get; set; }

        public virtual DbSet<ReqLogEntity> ReqLogEntity { get; set; }
        public virtual DbSet<CacheHitLogEntity> CacheHitLogEntity { get; set; }

        #endregion
    }
}
