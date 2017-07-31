using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc.auth;
using Lib.mvc.user;
using Dapper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Lib.data;
using Lib.helper;
using Lib.extension;
using Lib.core;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;

namespace Hiwjcn.Bll
{
    public class QipeilongLoginService : IAuthLoginService
    {
        private IDbConnection Database()
        {
            var con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"]?.ConnectionString);
            con.OpenIfClosedWithRetry(2);
            return con;
        }

        private LoginUserInfo Parse(UserInfo model)
        {
            return new LoginUserInfo()
            {
                IID = model.IID,
                UserID = model.UID,
                Email = model.Email,
                HeadImgUrl = model.Images,
                IsActive = model.IsActive ?? 0,
                IsValid = true,
                LoginToken = $"{model.IID}-{model.UID}-{model.UpdatedDate}".ToMD5()
            };
        }

        public async Task<LoginUserInfo> GetUserInfoByUID(string uid)
        {
            var user = default(LoginUserInfo);
            using (var con = Database())
            {
                var sql = "select top 1 * from parties.dbo.UserInfo where UID=@uid";
                var xx = (await con.QueryAsync<UserInfo>(sql, new { uid = uid })).FirstOrDefault();
                if (xx != null)
                {
                    user = this.Parse(xx);
                }
            }
            return user;
        }

        public async Task<(LoginUserInfo loginuser, string msg)> LoginByCode(string phoneOrEmail, string code)
        {
            using (var con = Database())
            {
                var time = DateTime.Now.AddSeconds(-300);
                var sql = "select top 1 * from parties.dbo.sms where recipient=@uname and createddate>=@time order by createddate desc";
                var sms = (await con.QueryAsync<Sms>(sql, new { uname = phoneOrEmail, time = time })).FirstOrDefault();
                if (sms == null || sms.MsgCode != code)
                {
                    return (null, "验证码错误");
                }
                sql = "select top 1 * from parties.dbo.userinfo where username=@uname";
                var user = (await con.QueryAsync<UserInfo>(sql, new { uname = phoneOrEmail })).FirstOrDefault();
                if (user == null)
                {
                    return (null, "用户不存在");
                }
                return (this.Parse(user), null);
            }
        }

        public async Task<(LoginUserInfo loginuser, string msg)> LoginByPassword(string user_name, string password)
        {
            (LoginUserInfo loginuser, string msg) data = (null, "没有实现");
            return await Task.FromResult(data);
        }

        public async Task<(LoginUserInfo loginuser, string msg)> LoginByToken(string token)
        {
            (LoginUserInfo loginuser, string msg) data = (null, "没有实现");
            return await Task.FromResult(data);
        }

        public async Task<string> SendOneTimeCode(string phoneOrEmail)
        {
            using (var con = Database())
            {
                var now = DateTime.Now;
                var ran = new Random((int)now.Ticks);
                var code = string.Empty.Join_(ran.Sample(Com.Range(10).ToList(), 4));
                //send sms

                var model = new Sms();
                model.UID = Com.GetUUID();
                model.MsgCode = code;
                model.Msg = $"汽配龙登录验证码：{code}";
                model.TemplateId = "no data";
                model.Recipient = phoneOrEmail;
                model.SmsType = 0;
                model.Status = (int)YesOrNoEnum.是;
                model.Sender = "大汉三通";
                model.CreatedDate = model.UpdatedDate = now;
                var sql = @"insert into parties.dbo.Sms 
(UID,MsgCode,Msg,TemplateId,Sender,Recipient,SmsType,IsRemove,CreatedDate,UpdatedDate,IsRead,Status) values 
(@UID,@MsgCode,@Msg,@TemplateId,@Sender,@Recipient,@SmsType,@IsRemove,@CreatedDate,@UpdatedDate,@IsRead,@Status)";
                if (await con.ExecuteAsync(sql, model) <= 0)
                {
                    return "发送验证码失败";
                }
                return string.Empty;
            }
        }
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
