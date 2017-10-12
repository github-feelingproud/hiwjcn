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
using QPL.WebService.TraderAccess.Core;

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

    public class TraderAccessServiceClient : ServiceClient<ITraderAccess>
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

            user.AddExtraData(nameof(model.UserType), model.UserType.ToString());

            user.IID = model.IID;
            user.UserID = model.UID;
            user.UserName = model.LoginNo;
            user.NickName = model.UserName;

            ////////////////////////////////////////////////////

            user.TraderId = trader.UID;
            user.TraderName = trader.ShopName;
            user.IsCheck = trader.IsCheck ?? 0;
            user.CustomerType = trader.CustomerType;
            user.IsSelf = trader.IsSelf ?? 0;
            user.IsHaveInquiry = trader.IsHaveInquiry ?? 0;
            user.IsActive = trader.IsActive ?? 0;
            user.IsGeneralDelivery = trader.IsGeneralDelivery ?? false;
            user.IsQuickArrive = trader.IsQuickArrive ?? false;
            user.MaxServiceDistance = trader.MaxServiceDistance ?? 0;
            user.Lon = trader.Lon ?? 0;
            user.Lat = (trader.Lat ?? "0").ToDouble();
            user.TraderShopType = trader.TraderShopType;

            user.AddExtraData(nameof(user.TraderId), user.TraderId);
            user.AddExtraData(nameof(trader.IsGeneralDelivery), (trader.IsGeneralDelivery ?? false).ToBoolInt().ToString());
            user.AddExtraData(nameof(trader.IsQuickArrive), (trader.IsQuickArrive ?? false).ToBoolInt().ToString());
            user.AddExtraData(nameof(trader.MaxServiceDistance), (trader.MaxServiceDistance ?? 0).ToString());
            user.AddExtraData(nameof(trader.Lon), (trader.Lon ?? 0).ToString());
            user.AddExtraData(nameof(trader.Lat), trader.Lat);
            user.AddExtraData(nameof(trader.TraderShopType), trader.TraderShopType);

            user.LoginToken = "等待设置token";

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
                if (user == null)
                {
                    return null;
                }

                var trader_list = await client.Instance.GetTradersByUids(new List<string>() { user.TraderId });
                var trader = trader_list.FirstOrDefault();
                if (trader == null)
                {
                    return null;
                }

                return await this.LoadPermissions(this.Parse(user, trader));
            }
        }

        public async Task<LoginUserInfo> LoadPermissions(LoginUserInfo model)
        {
            using (var client = new TraderAccessServiceClient())
            {
                var data = await client.Instance.SelectAccessList(new SelectAccessListRequest() { UID = model.UserID });
                var pers = data.AccessList;
                model.Permissions = new List<string>();
                model.Permissions.AddRange(pers.Where(x => ValidateHelper.IsPlumpString(x.ActionKey)).Select(x => $"action:{x.ActionKey}"));
                model.Permissions.AddRange(pers.Where(x => ValidateHelper.IsPlumpString(x.Url)).Select(x => $"url:{x.Url}".ToLower()));
                model.Permissions.AddRange(pers.Where(x => ValidateHelper.IsPlumpString(x.UID)).Select(x => $"uid:{x.UID}"));
                model.Permissions.AddRange(pers.Where(x => ValidateHelper.IsPlumpString(x.HtmlKey)).Select(x => $"html:{x.Url}{x.HtmlKey}".ToLower()));

                model.Permissions = model.Permissions.Distinct().ToList();
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
                if (res.User == null || res.Trader == null)
                {
                    data.SetErrorMsg("user或者trader为空");
                    return data;
                }
                data.SetSuccessData(await this.LoadPermissions(this.Parse(res.User, res.Trader)));
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
                data.DataList = new List<LoginUserInfo>();
                foreach (var u in response.UserList)
                {
                    var user = u;
                    if (user == null)
                    {
                        $"{nameof(TraderAccessLoginService)}：user为空".AddBusinessInfoLog();
                        continue;
                    }
                    var trader = trader_list.Where(x => x.UID == u.TraderId).FirstOrDefault();
                    if (trader == null)
                    {
                        $"{nameof(TraderAccessLoginService)}：user为空".AddBusinessInfoLog();
                        continue;
                    }
                    data.DataList.Add(this.Parse(user, trader));
                }
            }
            return data;
        }

        public Task<string> SendOneTimeCode(string phoneOrEmail)
        {
            throw new NotImplementedException("无法实现");
        }
    }
}
