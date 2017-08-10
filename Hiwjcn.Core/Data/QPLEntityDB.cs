using Hiwjcn.Dal;
using Lib.data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiwjcn.Core.Data
{
    public class QPLSqlServerConfiguration : DbConfiguration
    {
        public QPLSqlServerConfiguration()
        {
            this.SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
            this.SetDefaultConnectionFactory(new LocalDbConnectionFactory("mssqllocaldb"));
        }
    }

    [DbConfigurationType(typeof(QPLSqlServerConfiguration))]
    public class QipeilongDbContext : BaseEFContext
    {
        public QipeilongDbContext() : base(ConfigurationManager.ConnectionStrings["db_parties"]?.ConnectionString)
        {
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ValidateOnSaveEnabled = false;

            this.Database.CommandTimeout = 3;
            this.Database.Log = (log) =>
            {
                //if (!ValidateHelper.IsPlumpString(log)) { return; }
                //LogHelper.Error(typeof(EntityDB), "EF日志：\n" + log);
            };
        }

        public virtual DbSet<UserInfo> UserInfo { get; set; }
        public virtual DbSet<Sms> Sms { get; set; }
    }

    [Serializable]
    public class UserInfo : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IID { get; set; }
        public string UID { get; set; }
        public string UserName { get; set; }
        public string ShopNo { get; set; }
        public string ShopName { get; set; }
        public string CompanyName { get; set; }
        public string AgentPhone { get; set; }
        public string AgentManagerPhone { get; set; }
        public string Contact { get; set; }
        public Nullable<int> Sex { get; set; }
        public string IDcard { get; set; }
        public string Position { get; set; }
        public string Images { get; set; }
        public string Mobile { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string ProvinceId { get; set; }
        public string CityId { get; set; }
        public string TownId { get; set; }
        public string StreetId { get; set; }
        public string Email { get; set; }
        public string address { get; set; }
        public string Website { get; set; }
        public string Notes { get; set; }
        public Nullable<double> Lon { get; set; }
        public string Lat { get; set; }
        public string CustomerType { get; set; }
        public Nullable<int> IsActive { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<int> IsRemove { get; set; }
        public Nullable<int> IsCheck { get; set; }
        public string NoPassReason { get; set; }
        public string BusinessId { get; set; }
        public Nullable<System.DateTime> VerifyDate { get; set; }
        public string RepositoryUID { get; set; }
    }

    [Serializable]
    public class Sms : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IID { get; set; }
        public string UID { get; set; }
        public string MsgCode { get; set; }
        public string Msg { get; set; }
        public string TemplateId { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public Nullable<int> SmsType { get; set; }
        public int IsRemove { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public int IsRead { get; set; }
        public Nullable<int> Status { get; set; }
    }
}
