using Hiwjcn.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Core.Domain;
using Hiwjcn.Core.Model.Sys;
using WebLogic.Model.User;
using Model.User;

namespace Hiwjcn.Bll
{
    public class ClearDataBaseService : IClearDataBaseService
    {
        private readonly IRepository<AuthClient> _AuthClientRepo;
        private readonly IRepository<AuthScope> _AuthScopeRepo;
        private readonly IRepository<AuthToken> _AuthTokenRepo;
        private readonly IRepository<AuthCode> _AuthCodeRepo;
        private readonly IRepository<AuthTokenScope> _AuthTokenScopeRepo;

        private readonly IRepository<ReqLogModel> _ReqLogModelRepo;
        private readonly IRepository<CacheHitLog> _CacheHitLogRepo;

        private readonly IRepository<LoginErrorLogModel> _LoginErrorLogModelRepo;

        public ClearDataBaseService(
            IRepository<AuthClient> _AuthClientRepo,
            IRepository<AuthScope> _AuthScopeRepo,
            IRepository<AuthToken> _AuthTokenRepo,
            IRepository<AuthCode> _AuthCodeRepo,
            IRepository<AuthTokenScope> _AuthTokenScopeRepo,

            IRepository<ReqLogModel> _ReqLogModelRepo,
            IRepository<CacheHitLog> _CacheHitLogRepo,

            IRepository<LoginErrorLogModel> _LoginErrorLogModelRepo)
        {
            this._AuthClientRepo = _AuthClientRepo;
            this._AuthScopeRepo = _AuthScopeRepo;
            this._AuthTokenRepo = _AuthTokenRepo;
            this._AuthCodeRepo = _AuthCodeRepo;
            this._AuthTokenScopeRepo = _AuthTokenScopeRepo;

            this._ReqLogModelRepo = _ReqLogModelRepo;
            this._CacheHitLogRepo = _CacheHitLogRepo;

            this._LoginErrorLogModelRepo = _LoginErrorLogModelRepo;
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
                var user_set = db.Set<UserModel>();
                var client_set = db.Set<AuthClient>();

                var uids = user_set.Select(x => x.UID);
                var range = client_set.Where(x => !uids.Contains(x.UserUID));
                client_set.RemoveRange(range);

                db.SaveChanges();

                return true;
            });
        }

        public void ClearLoginLog()
        {
            var expire = DateTime.Now.AddDays(-100);
            this._LoginErrorLogModelRepo.DeleteWhere(x => x.CreateTime < expire);
        }

        public void ClearPage()
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
            this._LoginErrorLogModelRepo.PrepareSession(db =>
            {
                var user_set = db.Set<UserModel>();
                var role_set = db.Set<RoleModel>();
                var role_map_set = db.Set<UserRoleModel>();

                var user_uids = user_set.Select(x => x.UID);
                var role_uids = role_set.Select(x => x.UID);
                var range = role_map_set.Where(x => (!user_uids.Contains(x.UserID)) || (!role_uids.Contains(x.RoleID)));
                role_map_set.RemoveRange(range);

                db.SaveChanges();

                return true;
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

                return true;
            });
        }

        public void ClearTag()
        {
            //
        }

        public void ClearToken()
        {
            var expire = DateTime.Now.AddMonths(-3);
            this._AuthTokenRepo.PrepareSession(db =>
            {
                var user_set = db.Set<UserModel>();
                var token_set = db.Set<AuthToken>();
                var client_set = db.Set<AuthClient>();

                var user_uids = user_set.Select(x => x.UID);
                var client_uids = client_set.Select(x => x.UID);
                var range = token_set.Where(x => x.CreateTime < expire || (!user_uids.Contains(x.UserUID)) || (!client_uids.Contains(x.ClientUID)));
                token_set.RemoveRange(range);

                db.SaveChanges();

                return true;
            });
        }

        public void ClearUser()
        {
            //
        }
    }
}
