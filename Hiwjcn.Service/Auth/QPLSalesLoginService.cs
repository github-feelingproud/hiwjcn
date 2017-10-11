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
using Lib.rpc;
using QPL.WebService.TraderAccess.Core.Model;
using QPL.WebService.TraderAccess.Core.Request;

namespace Hiwjcn.Bll.Auth
{
    [Obsolete("sales登录逻辑修改了，用阿杜的逻辑：" + nameof(TraderAccessLoginService))]
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

    public class TraderAccessServiceClient : ServiceClient<QPL.WebService.TraderAccess.Core.ITraderAccess>
    {
        public TraderAccessServiceClient() : base(ConfigurationManager.AppSettings["TraderServiceUrl"] ??
            throw new Exception("请在appsetting中配置traderaccess服务地址"))
        { }
    }

    public class TraderAccessLoginService : IAuthLoginService
    {
        private LoginUserInfo Parse(UserModel model, TraderModel trader)
        {
            var user = new LoginUserInfo();

            user.AddExtraData("NewUserId", model.UID);
            user.AddExtraData(nameof(model.UserType), model.UserType.ToString());

            if (trader != null)
            {
                user.NickName = trader.UserName;
                user.UserID = trader.UID;
                user.TraderName = trader.ShopName;
                user.IsCheck = trader.IsCheck ?? 0;
                user.CustomerType = trader.CustomerType;
                user.IsSelf = trader.IsSelf ?? 0;
                user.IsHaveInquiry = trader.IsHaveInquiry ?? 0;
                user.IsActive = trader.IsActive ?? 0;
                user.AddExtraData(nameof(trader.IsGeneralDelivery), (trader.IsGeneralDelivery ?? false).ToBoolInt().ToString());
                user.AddExtraData(nameof(trader.IsQuickArrive), (trader.IsQuickArrive ?? false).ToBoolInt().ToString());
                user.AddExtraData(nameof(trader.MaxServiceDistance), (trader.MaxServiceDistance ?? 0).ToString());
                user.AddExtraData(nameof(trader.Lon), (trader.Lon ?? 0).ToString());
                user.AddExtraData(nameof(trader.Lat), trader.Lat);
                user.AddExtraData(nameof(trader.TraderShopType), trader.TraderShopType);
            }

            return user;
        }

        public async Task<LoginUserInfo> GetUserInfoByUID(string uid)
        {
            using (var client = new TraderAccessServiceClient())
            {
                var list = await client.Instance.SelectUserInfo(new SelectUserInfoRequest()
                {
                    UID = uid,
                    PageIndex = 1,
                    PageSize = 1
                });
                var user = list.UserList.FirstOrDefault();
                var trader_list = await client.Instance.GetTradersByUids(new List<string>() { user.TraderId });
                var trader = trader_list.FirstOrDefault();
                return this.Parse(user, trader);
            }
        }

        public async Task<LoginUserInfo> LoadPermissions(LoginUserInfo model)
        {
            using (var client = new TraderAccessServiceClient())
            {
                var data = await client.Instance.SelectAccessList(new SelectAccessListRequest() { UID = model.GetExtraData("NewUserId") });
                var pers = data.AccessList;
                model.Permissions = new List<string>();
                model.Permissions.AddRange(pers.Select(x => $"action:{x.ActionKey}"));
                model.Permissions.AddRange(pers.Select(x => $"url:{x.Url}"));
                model.Permissions.AddRange(pers.Select(x => $"uid:{x.UID}"));
                return model;
            }
        }

        public Task<_<LoginUserInfo>> LoginByCode(string phoneOrEmail, string code)
        {
            throw new NotImplementedException("无法实现");
        }

        public async Task<_<LoginUserInfo>> LoginByPassword(string user_name, string password)
        {
            var data = new _<LoginUserInfo>();
            using (var client = new TraderAccessServiceClient())
            {
                var res = await client.Instance.VerifyLogOn(new VerifyLogOnRequest()
                {
                    LoginNo = user_name,
                    Password = password.ToMD5(),
                    PlateNo = 64,
                    Ip = "127.0.0.1"
                });
                if (!res.IsSuccess)
                {
                    data.SetErrorMsg(res.Message);
                    return data;
                }
                data.SetSuccessData(this.Parse(res.User, res.Trader));
            }
            return data;
        }

        public async Task<PagerData<LoginUserInfo>> SearchUser(string q = null, int page = 1, int pagesize = 10)
        {
            var data = new PagerData<LoginUserInfo>();
            using (var client = new TraderAccessServiceClient())
            {
                var response = await client.Instance.SelectUserInfo(new SelectUserInfoRequest()
                {
                    PageIndex = page,
                    PageSize = pagesize,
                    LoginNo = q
                });

                var trader_list = await client.Instance.GetTradersByUids(response.UserList.Select(x => x.TraderId).ToList());

                data.ItemCount = response.Total;
                data.DataList = response.UserList.Select(x => this.Parse(x, trader_list.Where(m => m.UID == x.TraderId).FirstOrDefault())).ToList();
            }
            return data;
        }

        public Task<string> SendOneTimeCode(string phoneOrEmail)
        {
            throw new NotImplementedException("无法实现");
        }
    }
}
