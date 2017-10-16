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
    public enum AccountSystemEnum : int
    {
        UserInfo = 1,
        SalesInfo = 2,
        TraderAccess = 3
    }

    public class QPLSqlServerConfiguration : DbConfiguration
    {
        public QPLSqlServerConfiguration()
        {
            this.SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
            this.SetDefaultConnectionFactory(new LocalDbConnectionFactory("mssqllocaldb"));
        }
    }

    [DbConfigurationType(typeof(QPLSqlServerConfiguration))]
    public class QPLEntityDB : BaseEFContext
    {
        public QPLEntityDB() : base(
            ConfigurationManager.ConnectionStrings["db_parties"]?.ConnectionString ??
            throw new Exception("请配置parties数据库链接字符串"))
        {
            //
        }

        public virtual DbSet<UserInfo> UserInfo { get; set; }
        public virtual DbSet<Sms> Sms { get; set; }
        public virtual DbSet<TraderInfo> TraderInfo { get; set; }
        public virtual DbSet<T_SalesInfo> T_SalesInfo { get; set; }
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

    [Serializable]
    public class TraderInfo : Lib.data.IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IID { get; set; }
        public string UID { get; set; }
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public string ShopName { get; set; }
        public string TraderNo { get; set; }
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
        public decimal? Lon { get; set; }
        public decimal? Lat { get; set; }
        public string CustomerType { get; set; }
        public int IsActive { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public int IsRemove { get; set; }
        public int IsCheck { get; set; }
        public string BelongTraderId { get; set; }
        public int IsVAT { get; set; }
        public int SettlementIntervalx { get; set; }
        public Nullable<int> WarningValue { get; set; }
        public int IsSelf { get; set; }
        public int FavoriteCount { get; set; }
        public Nullable<int> IsCarParts { get; set; }
        public Nullable<int> IsCommon { get; set; }
        public Nullable<int> IsHaveInquiry { get; set; }

        #region 扩展
        [NotMapped]
        public virtual string SiteID { get; set; }

        [NotMapped]
        public virtual string SellerID { get; set; }

        [NotMapped]
        public virtual string SettingID { get; set; }
        #endregion
    }

    [Serializable]
    public partial class T_SalesInfo : Lib.data.IDBTable
    {
        [Key]
        public string UID { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string Pwd { get; set; }
        public Nullable<int> WorkingStatus { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<int> IsRemove { get; set; }
        public Nullable<int> IsActive { get; set; }
        public Nullable<int> ServerOrderList { get; set; }
    }
}
