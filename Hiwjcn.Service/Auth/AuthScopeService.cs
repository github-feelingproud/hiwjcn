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
            return (await this._AuthScopeRepository.GetListAsync(x => x.IsRemove <= 0)).OrderByDescending(x => x.Important).OrderBy(x => x.Name).ToList();
        }

        public async Task<string> AddScopeAsync(AuthScope scope)
        {
            scope.Init();
            if (!this.CheckModel(scope, out var msg))
            {
                return msg;
            }
            scope.Name = scope.Name.ToLower();

            if (await this._AuthScopeRepository.ExistAsync(x => x.Name == scope.Name))
            {
                return "scope name已存在";
            }

            if (await this._AuthScopeRepository.AddAsync(scope) > 0)
            {
                this._publisher.EntityInserted(scope);
                return SUCCESS;
            }
            throw new MsgException("保存scope异常");
        }

        public async Task<string> DeleteScope(string scope_uid)
        {
            var scope = await this._AuthScopeRepository.GetFirstAsync(x => x.UID == scope_uid);
            Com.Assert(scope, x => x != null, $"找不到scope[scope_uid={scope_uid}]");
            if (await this._AuthScopeRepository.DeleteAsync(scope) > 0)
            {
                this._publisher.EntityDeleted(scope);
                return SUCCESS;
            }
            throw new MsgException("删除scope失败");
        }

        public async Task<string> UpdateScope(AuthScope updatemodel)
        {
            var scope = await this._AuthScopeRepository.GetFirstAsync(x => x.UID == updatemodel.UID);
            Com.Assert(scope, x => x != null, $"找不到scope[scope_uid={updatemodel.UID}]");

            //scope.Name = updatemodel.Name;
            scope.DisplayName = updatemodel.DisplayName;
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
            throw new MsgException("更新scope失败");
        }

        public override string CheckModel(AuthScope model)
        {
            return base.CheckModel(model);
        }

    }
}
