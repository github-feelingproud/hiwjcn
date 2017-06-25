using Hiwjcn.Core.Infrastructure.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using Lib.infrastructure;
using Hiwjcn.Core.Domain.Auth;
using Lib.core;
using Lib.helper;

namespace Hiwjcn.Bll.Auth
{
    public class AuthTokenService : ServiceBase<AuthToken>, IAuthTokenService
    {
        private readonly IRepository<AuthToken> _AuthTokenRepository;
        private readonly IRepository<AuthTokenScope> _AuthTokenScopeRepository;
        private readonly IRepository<AuthScope> _AuthScopeRepository;
        private readonly IRepository<AuthCode> _AuthCodeRepository;
        public AuthTokenService(
            IRepository<AuthToken> _AuthTokenRepository,
            IRepository<AuthTokenScope> _AuthTokenScopeRepository,
            IRepository<AuthScope> _AuthScopeRepository,
            IRepository<AuthCode> _AuthCodeRepository)
        {
            this._AuthTokenRepository = _AuthTokenRepository;
            this._AuthTokenScopeRepository = _AuthTokenScopeRepository;
            this._AuthScopeRepository = _AuthScopeRepository;
            this._AuthCodeRepository = _AuthCodeRepository;
        }

        public override string CheckModel(AuthToken model)
        {
            return base.CheckModel(model);
        }

        public virtual async Task<string> CreateToken(AuthToken token)
        {
            if (!this.CheckModel(token, out var msg))
            {
                return msg;
            }
            if (!ValidateHelper.IsPlumpList(token.ScopeNames))
            {
                token.ScopeNames = (await this._AuthScopeRepository.GetListAsync(x => x.IsDefault > 0)).Select(x => x.Name).ToList();
                if (!ValidateHelper.IsPlumpList(token.ScopeNames))
                {
                    throw new MsgException("没有设置默认scope");
                }
            }
            else
            {
                var count = await this._AuthScopeRepository.GetCountAsync(x => token.ScopeNames.Contains(x.Name));
                if (count != token.ScopeNames.Count)
                {
                    throw new MsgException("提交数据错误");
                }
            }
            if ((await this._AuthTokenRepository.AddAsync(token)) <= 0)
            {
                throw new MsgException("保存token失败");
            }
            var scopes = token.ScopeNames.Select(x => new AuthTokenScope()
            {
                UID = Com.GetUUID(),
                CreateTime = DateTime.Now
            }).ToArray();
            if ((await this._AuthTokenScopeRepository.AddAsync(scopes)) <= 0)
            {
                throw new MsgException("保存token scope map失败");
            }
            return SUCCESS;
        }

        public Task<AuthToken> FindToken(string token_uid)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<AuthToken> RefreshToken(string refresh_token_uid)
        {
            var token = await this._AuthTokenRepository.GetFirstAsync(x => x.RefreshToken == refresh_token_uid && x.ExpiryTime < DateTime.Now) ?? throw new SourceNotExistException();
            token.ExpiryTime = DateTime.Now.AddDays(30);
            if (!this.CheckModel(token, out var msg))
            {
                throw new MsgException(msg);
            }
            if ((await this._AuthTokenRepository.UpdateAsync(token)) <= 0)
            {
                throw new MsgException("更新token失败");
            }
            return token;
        }
    }
}
