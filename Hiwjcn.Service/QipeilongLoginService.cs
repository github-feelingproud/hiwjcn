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

namespace Hiwjcn.Bll
{
    public class QipeilongLoginService : IAuthLoginService
    {
        public Task<LoginUserInfo> GetUserInfoByUID(string uid)
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> GetUserPermission(string uid)
        {
            return await Task.FromResult(new List<string>() { "basic_info" });
        }

        public Task<LoginUserInfo> LoginByCode(string phoneOrEmail, string code)
        {
            throw new NotImplementedException();
        }

        public Task<LoginUserInfo> LoginByPassword(string user_name, string password)
        {
            throw new NotImplementedException();
        }

        public Task<LoginUserInfo> LoginByToken(string token)
        {
            throw new NotImplementedException();
        }

        public Task<string> SendOneTimeCode(string phoneOrEmail)
        {
            throw new NotImplementedException();
        }
    }

    public class UserInfo
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

    public class Sms
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
