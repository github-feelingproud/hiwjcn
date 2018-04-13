using Hiwjcn.Core.Data;
using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Core.Domain.Sys;
using Hiwjcn.Core.Domain.User;
using Lib.data.ef;
using Lib.extension;
using Lib.ioc;
using System;
using System.Linq;

namespace Hiwjcn.Service.MemberShip
{
    public interface IClearDataBaseService : IAutoRegistered
    {
        void ClearCacheHitLog();

        void ClearClient();

        void ClearLoginLog();

        void ClearPage();

        void ClearPermission();

        void ClearRequestLog();

        void ClearRole();

        void ClearScope();

        void ClearToken();

        void ClearUser();
    }

    public class ClearDataBaseService : IClearDataBaseService
    {
        private readonly IMSRepository<AuthToken> _AuthTokenRepo;

        private readonly IMSRepository<ReqLogEntity> _ReqLogModelRepo;
        private readonly IMSRepository<CacheHitLogEntity> _CacheHitLogRepo;

        public ClearDataBaseService(
            IMSRepository<AuthToken> _AuthTokenRepo,

            IMSRepository<ReqLogEntity> _ReqLogModelRepo,
            IMSRepository<CacheHitLogEntity> _CacheHitLogRepo)
        {
            this._AuthTokenRepo = _AuthTokenRepo;

            this._ReqLogModelRepo = _ReqLogModelRepo;
            this._CacheHitLogRepo = _CacheHitLogRepo;
        }

        public void ClearCacheHitLog()
        {
            var expire = DateTime.Now.AddDays(-30);
            this._CacheHitLogRepo.DeleteWhere(x => x.CreateTime < expire);
        }

        public void ClearClient()
        {
            //
        }

        public void ClearLoginLog()
        {
            var expire = DateTime.Now.AddDays(-100);
        }

        public void ClearPage()
        {
            "页面的数据不会被清理".AddBusinessInfoLog();
        }

        public void ClearPermission()
        {
            //
        }

        public void ClearRequestLog()
        {
            var expire = DateTime.Now.AddDays(-30);
            this._ReqLogModelRepo.DeleteWhere(x => x.CreateTime < expire);
        }

        public void ClearRole()
        {
            //
        }

        public void ClearScope()
        {
            //
        }

        public void ClearToken()
        {
            //
        }

        public void ClearUser()
        {
            "用户信息不会被清理".AddBusinessInfoLog();
        }
    }
}
