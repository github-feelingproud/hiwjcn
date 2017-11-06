using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;
using Lib.data;
using Lib.mvc;

namespace Lib.infrastructure.service
{
    public interface IAuthServiceBase<ClientBase, ScopeBase, TokenBase, CodeBase, TokenScopeBase>
        where ClientBase : AuthClientBase, new()
        where ScopeBase : AuthScopeBase, new()
        where TokenBase : AuthTokenBase, new()
        where CodeBase : AuthCodeBase, new()
        where TokenScopeBase : AuthTokenScopeBase, new()
    {
        Task<TokenBase> FindTokenAsync(string client_uid, string token_uid);
        Task<_<TokenBase>> CreateTokenAsync(string client_id, string client_secret, string code_uid);
    }

    public abstract class AuthServiceBase<ClientBase, ScopeBase, TokenBase, CodeBase, TokenScopeBase> : IAuthServiceBase<ClientBase, ScopeBase, TokenBase, CodeBase, TokenScopeBase>
        where ClientBase : AuthClientBase, new()
        where ScopeBase : AuthScopeBase, new()
        where TokenBase : AuthTokenBase, new()
        where CodeBase : AuthCodeBase, new()
        where TokenScopeBase : AuthTokenScopeBase, new()
    {
        [Obsolete("用另外一个构造函数注入对象")]
        public AuthServiceBase() { }

        protected readonly IRepository<ClientBase> _clientRepo;
        protected readonly IRepository<ScopeBase> _scopeRepo;
        protected readonly IRepository<TokenBase> _tokenRepo;
        protected readonly IRepository<CodeBase> _codeRepo;
        protected readonly IRepository<TokenScopeBase> _tokenScopeRepo;

        public AuthServiceBase(
            IRepository<ClientBase> _clientRepo,
            IRepository<ScopeBase> _scopeRepo,
            IRepository<TokenBase> _tokenRepo,
            IRepository<CodeBase> _codeRepo,
            IRepository<TokenScopeBase> _tokenScopeRepo)
        {
            this._clientRepo = _clientRepo;
            this._scopeRepo = _scopeRepo;
            this._tokenRepo = _tokenRepo;
            this._codeRepo = _codeRepo;
            this._tokenScopeRepo = _tokenScopeRepo;
        }

        public Task<TokenBase> FindTokenAsync(string client_uid, string token_uid)
        {
            throw new NotImplementedException();
        }

        public Task<_<TokenBase>> CreateTokenAsync(string client_id, string client_secret, string code_uid)
        {
            throw new NotImplementedException();
        }
    }
}
