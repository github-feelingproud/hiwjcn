using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc.auth;
using Lib.mvc.user;
using Lib.mvc;
using Lib.net;
using Dapper;
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
using Hiwjcn.Core.Data;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using Polly;
using Polly.CircuitBreaker;

namespace Hiwjcn.Bll.Auth
{
    public class QPLSalesLoginService : IAuthLoginService
    {
        private LoginUserInfo Parse(T_SalesInfo model)
        {
            var loginuser = new LoginUserInfo() { };

            loginuser.MapExtraData(model);
            return loginuser;
        }

        private bool CheckUser(T_SalesInfo model, out string msg)
        {
            if (model == null)
            {
                msg = "用户不存在";
                return false;
            }
            if (model.IsActive == null || model.IsActive <= 0 || model.IsRemove == null || model.IsRemove > 0)
            {
                msg = "用户未激活或者被删除";
                return false;
            }

            msg = string.Empty;
            return true;
        }

        public async Task<LoginUserInfo> GetUserInfoByUID(string uid)
        {
            using (var db = new QPLEntityDB())
            {
                var model = await db.T_SalesInfo.Where(x => x.UID == uid).FirstOrDefaultAsync();
                if (model != null)
                {
                    return await this.LoadPermissions(this.Parse(model));
                }
            }
            return null;
        }

        public async Task<LoginUserInfo> LoadPermissions(LoginUserInfo model)
        {
            return await Task.FromResult(model);
        }

        public Task<_<LoginUserInfo>> LoginByCode(string phoneOrEmail, string code)
        {
            throw new NotImplementedException("销售账号不支持验证码登录");
        }

        public async Task<_<LoginUserInfo>> LoginByPassword(string user_name, string password)
        {
            var data = new _<LoginUserInfo>();
            using (var db = new QPLEntityDB())
            {
                var model = await db.T_SalesInfo.Where(x => x.UserName == user_name || x.NickName == user_name).FirstOrDefaultAsync();
                if (model == null)
                {
                    data.SetErrorMsg("销售不存在");
                    return data;
                }
                data.SetSuccessData(await this.LoadPermissions(this.Parse(model)));
            }
            return data;
        }

        public async Task<PagerData<LoginUserInfo>> SearchUser(string q = null, int page = 1, int pagesize = 10)
        {
            var data = new PagerData<LoginUserInfo>();
            using (var db = new QPLEntityDB())
            {
                var query = db.T_SalesInfo.AsNoTrackingQueryable();

                if (ValidateHelper.IsPlumpString(q))
                {
                    query = query.Where(x => x.NickName.Contains(q) || x.UserName.Contains(q) || x.UserID == q || x.UID == q);
                }

                data.ItemCount = await query.CountAsync();
                data.DataList = (await query.OrderByDescending(x => x.UpdatedDate).QueryPage(page, pagesize).ToListAsync()).Select(x => this.Parse(x)).ToList();
            }
            return data;
        }

        public Task<string> SendOneTimeCode(string phoneOrEmail)
        {
            throw new NotImplementedException("销售账号不支持发送验证码");
        }
    }
}
