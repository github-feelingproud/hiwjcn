using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Core.Infrastructure.Auth;
using Lib.infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using Lib.helper;
using Lib.events;
using Lib.core;
using Lib.extension;

namespace Hiwjcn.Bll.Auth
{
    public class AuthScopeService : ServiceBase<AuthScope>, IAuthScopeService
    {
        private readonly IEventPublisher _publisher;

        private readonly IRepository<AuthScope> _AuthScopeRepository;
        public AuthScopeService(
            IEventPublisher _publisher,
            IRepository<AuthScope> _AuthScopeRepository)
        {
            this._publisher = _publisher;

            this._AuthScopeRepository = _AuthScopeRepository;
        }

        public async Task<List<AuthScope>> GetScopesOrDefault(params string[] names)
        {
            if (!ValidateHelper.IsPlumpList(names))
            {
                var deft = await this._AuthScopeRepository.GetListAsync(x => x.IsDefault > 0);
                if (!ValidateHelper.IsPlumpList(deft))
                {
                    "没有设置默认scope".AddBusinessInfoLog();
                }
                return deft;
            }
            return await this._AuthScopeRepository.GetListAsync(x => names.Contains(x.Name));
        }

        public async Task<List<AuthScope>> AllScopes()
        {
            return (await this._AuthScopeRepository.GetListAsync(null)).OrderByDescending(x => x.Important).OrderBy(x => x.Name).ToList();
        }

        public async Task<string> AddScopeAsync(AuthScope scope)
        {
            scope.UID = Com.GetUUID();
            scope.CreateTime = DateTime.Now;
            scope.UpdateTime = null;
            if (!this.CheckModel(scope, out var msg))
            {
                return msg;
            }
            if (await this._AuthScopeRepository.AddAsync(scope) > 0)
            {
                this._publisher.EntityInserted(scope);
                return SUCCESS;
            }
            return "保存失败";
        }

        public async Task<string> DeleteScope(string scope_uid)
        {
            var scope = await this._AuthScopeRepository.GetFirstAsync(x => x.UID == scope_uid);
            if (scope == null)
            {
                return "scope不存在";
            }
            if (await this._AuthScopeRepository.DeleteAsync(scope) > 0)
            {
                this._publisher.EntityDeleted(scope);
                return SUCCESS;
            }
            return "删除scope失败";
        }

        public async Task<string> UpdateScope(AuthScope updatemodel)
        {
            var scope = await this._AuthScopeRepository.GetFirstAsync(x => x.UID == updatemodel.UID);
            if (scope == null)
            {
                return "scope不存在";
            }

            scope.Name = updatemodel.Name;
            scope.Description = updatemodel.Description;
            scope.Important = updatemodel.Important;
            scope.Sort = updatemodel.Sort;
            scope.IsDefault = updatemodel.IsDefault;
            scope.ImageUrl = updatemodel.ImageUrl;
            scope.FontIcon = updatemodel.FontIcon;
            scope.UpdateTime = DateTime.Now;

            if (await this._AuthScopeRepository.UpdateAsync(scope) > 0)
            {
                this._publisher.EntityUpdated(scope);
                return SUCCESS;
            }
            return "更新scope失败";
        }

        public override string CheckModel(AuthScope model)
        {
            return base.CheckModel(model);
        }

    }
}
