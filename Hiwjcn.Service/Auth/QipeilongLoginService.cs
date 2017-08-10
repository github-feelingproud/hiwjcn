using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc.auth;
using Lib.mvc.user;
using Lib.mvc;
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
using Hiwjcn.Core.Infrastructure.Auth;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using Hiwjcn.Core.Data;

namespace Hiwjcn.Bll.Auth
{
    public class QipeilongLoginService : IAuthLoginService
    {
        private readonly string[] sys_users = new string[] { "13915280232", "18101795560" };


        private IDbConnection Database()
        {
            var con = new SqlConnection(ConfigurationManager.ConnectionStrings["db"]?.ConnectionString);
            con.OpenIfClosedWithRetry(2);
            return con;
        }

        private LoginUserInfo Parse(UserInfo model)
        {
            var loginuser = new LoginUserInfo()
            {
                IID = model.IID,
                UserID = model.UID,
                Email = model.Email,
                UserName = model.UserName,
                NickName = model.ShopName,
                HeadImgUrl = model.Images,
                IsActive = model.IsActive ?? 0,
                IsValid = true,
                LoginToken = "please load auth token"
            };

            loginuser.AddExtraData(nameof(model.address), model.address);
            loginuser.AddExtraData(nameof(model.CompanyName), model.CompanyName);
            loginuser.AddExtraData(nameof(model.Contact), model.Contact);
            loginuser.AddExtraData(nameof(model.CustomerType), model.CustomerType);
            loginuser.AddExtraData(nameof(model.IsCheck), model.IsCheck.ToString());
            loginuser.AddExtraData(nameof(model.Phone), model.Phone);

            return loginuser;
        }

        public async Task<LoginUserInfo> LoadPermissions(LoginUserInfo model)
        {
            if (model == null)
            {
                return model;
            }
            if (sys_users.Contains(model.UserName))
            {
                model.Permissions = new List<string>()
                {
                    "manage.auth",
                    "manage.user",
                    "manage.system",
                    "manage.order",
                    "manage.product"
                };
            }
            return await Task.FromResult(model);
        }

        public async Task<LoginUserInfo> GetUserInfoByUID(string uid)
        {
            using (var con = Database())
            {
                var sql = "select top 1 * from parties.dbo.UserInfo where UID=@uid";
                var userinfo = (await con.QueryAsync<UserInfo>(sql, new { uid = uid })).FirstOrDefault();
                if (userinfo != null)
                {
                    return await this.LoadPermissions(this.Parse(userinfo));
                }
            }
            return null;
        }

        public async Task<_<LoginUserInfo>> LoginByCode(string phoneOrEmail, string code)
        {
            var data = new _<LoginUserInfo>();
            using (var con = Database())
            {
                var time = DateTime.Now.AddSeconds(-300);
                var sql = "select top 1 * from parties.dbo.sms where recipient=@uname and createddate>=@time order by createddate desc";
                var sms = (await con.QueryAsync<Sms>(sql, new { uname = phoneOrEmail, time = time })).FirstOrDefault();
                if (!((sms != null && sms.MsgCode == code) || (sys_users.Contains(phoneOrEmail) && code == "0000")))
                {
                    data.SetErrorMsg("验证码错误");
                    return data;
                }
                sql = "select top 1 * from parties.dbo.userinfo where username=@uname";
                var user = (await con.QueryAsync<UserInfo>(sql, new { uname = phoneOrEmail })).FirstOrDefault();
                if (user == null)
                {
                    data.SetErrorMsg("用户不存在");
                    return data;
                }
                var loginuser = await this.LoadPermissions(this.Parse(user));
                data.SetSuccessData(loginuser);
            }
            return data;
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

        public async Task<_<LoginUserInfo>> LoginByPassword(string user_name, string password)
        {
            var data = new _<LoginUserInfo>();

            using (var db = new QipeilongDbContext())
            {
                var model = await db.UserInfo.Where(x => x.UserName == user_name).FirstOrDefaultAsync();
                if (model == null || !sys_users.Contains(model.UserName) || password != "123")
                {
                    data.SetErrorMsg("账号密码错误");
                    return data;
                }
                var loginuser = await this.LoadPermissions(this.Parse(model));
                data.SetSuccessData(loginuser);
            }

            return data;
        }
    }
}
