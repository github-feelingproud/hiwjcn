using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Core.Domain.Sys;
using Hiwjcn.Core.Domain.User;
using Lib.data.ef;
using Lib.extension;
using Lib.ioc;
using System;
using System.Linq;

namespace Hiwjcn.Bll
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
        private readonly IEFRepository<AuthClient> _AuthClientRepo;
        private readonly IEFRepository<AuthScope> _AuthScopeRepo;
        private readonly IEFRepository<AuthToken> _AuthTokenRepo;
        private readonly IEFRepository<AuthCode> _AuthCodeRepo;
        private readonly IEFRepository<AuthTokenScope> _AuthTokenScopeRepo;

        private readonly IEFRepository<ReqLogEntity> _ReqLogModelRepo;
        private readonly IEFRepository<CacheHitLogEntity> _CacheHitLogRepo;

        public ClearDataBaseService(
            IEFRepository<AuthClient> _AuthClientRepo,
            IEFRepository<AuthScope> _AuthScopeRepo,
            IEFRepository<AuthToken> _AuthTokenRepo,
            IEFRepository<AuthCode> _AuthCodeRepo,
            IEFRepository<AuthTokenScope> _AuthTokenScopeRepo,

            IEFRepository<ReqLogEntity> _ReqLogModelRepo,
            IEFRepository<CacheHitLogEntity> _CacheHitLogRepo)
        {
            this._AuthClientRepo = _AuthClientRepo;
            this._AuthScopeRepo = _AuthScopeRepo;
            this._AuthTokenRepo = _AuthTokenRepo;
            this._AuthCodeRepo = _AuthCodeRepo;
            this._AuthTokenScopeRepo = _AuthTokenScopeRepo;

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
            this._AuthClientRepo.PrepareSession(db =>
            {
                var user_set = db.Set<UserEntity>();
                var client_set = db.Set<AuthClient>();

                var uids = user_set.Select(x => x.UID);
                var range = client_set.Where(x => !uids.Contains(x.UserUID));
                client_set.RemoveRange(range);

                db.SaveChanges();
            });
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
            this._AuthClientRepo.PrepareSession(db =>
            {
                var role_set = db.Set<RoleEntity>();
                var permission_set = db.Set<PermissionEntity>();
                var permission_map = db.Set<RolePermissionEntity>();

                var role_uids = role_set.Select(x => x.UID);
                var permission_uids = permission_set.Select(x => x.UID);
                var range = permission_map.Where(x => !role_uids.Contains(x.RoleID) || !permission_uids.Contains(x.PermissionID));

                permission_map.RemoveRange(range);

                db.SaveChanges();
            });
        }

        public void ClearRequestLog()
        {
            var expire = DateTime.Now.AddDays(-30);
            this._ReqLogModelRepo.DeleteWhere(x => x.CreateTime < expire);
        }

        public void ClearRole()
        {
            this._AuthClientRepo.PrepareSession(db =>
            {
                var user_set = db.Set<UserEntity>();
                var role_set = db.Set<RoleEntity>();
                var role_map_set = db.Set<UserRoleEntity>();

                var user_uids = user_set.Select(x => x.UID);
                var role_uids = role_set.Select(x => x.UID);
                var range = role_map_set.Where(x => !user_uids.Contains(x.UserID) || !role_uids.Contains(x.RoleID));
                role_map_set.RemoveRange(range);

                db.SaveChanges();
            });
        }

        public void ClearScope()
        {
            this._AuthScopeRepo.PrepareSession(db =>
            {
                var token_set = db.Set<AuthToken>();
                var scope_set = db.Set<AuthTokenScope>();

                var uids = token_set.Select(x => x.UID);
                var range = scope_set.Where(x => !uids.Contains(x.TokenUID));

                scope_set.RemoveRange(range);

                db.SaveChanges();
            });
        }

        public void ClearToken()
        {
            var now = DateTime.Now;
            this._AuthTokenRepo.PrepareSession(db =>
            {
                var user_set = db.Set<UserEntity>();
                var token_set = db.Set<AuthToken>();
                var client_set = db.Set<AuthClient>();

                var user_uids = user_set.Select(x => x.UID);
                var client_uids = client_set.Select(x => x.UID);
                var range = token_set.Where(x => x.ExpiryTime < now || (!user_uids.Contains(x.UserUID)) || (!client_uids.Contains(x.ClientUID)));
                token_set.RemoveRange(range);

                db.SaveChanges();
            });
        }

        public void ClearUser()
        {
            "用户信息不会被清理".AddBusinessInfoLog();
        }
    }
}
