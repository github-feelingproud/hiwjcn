#define use_mysql

using Hiwjcn.Core.Domain;
using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Core.Domain.Sys;
using Hiwjcn.Core.Domain.User;
using Hiwjcn.Core.Entity;
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
    /// EF容器上下文
    /// EF做映射的时候也可以数据库是int，代码里是string，
    /// 用attribute mapping测试成功，用fluent测试失败
    /// </summary>
#if use_mysql_
    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
#else
    [DbConfigurationType(typeof(Hiwjcn.Core.SqlServerConfiguration))]
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

        public virtual DbSet<UserAvatarEntity> UserAvatar { get; set; }
        public virtual DbSet<UserEntity> UserEntity { get; set; }
        public virtual DbSet<UserOneTimeCodeEntity> UserOneTimeCode { get; set; }

        public virtual DbSet<RoleEntity> RoleEntity { get; set; }
        public virtual DbSet<RolePermissionEntity> RolePermissionEntity { get; set; }
        public virtual DbSet<UserRoleEntity> UserRoleEntity { get; set; }

        public virtual DbSet<PermissionEntity> PermissionEntity { get; set; }

        public virtual DbSet<DepartmentEntity> DepartmentEntity { get; set; }
        public virtual DbSet<UserDepartmentEntity> UserDepartmentEntity { get; set; }
        public virtual DbSet<DepartmentRoleEntity> DepartmenetRoleEntity { get; set; }

        public virtual DbSet<ReqLogEntity> ReqLogEntity { get; set; }
        public virtual DbSet<CacheHitLogEntity> CacheHitLogEntity { get; set; }

        public virtual DbSet<MenuEntity> MenuEntity { get; set; }

        public virtual DbSet<SystemEntity> SystemEntity { get; set; }

        #endregion
    }
}
