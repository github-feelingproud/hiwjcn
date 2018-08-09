using Microsoft.AspNetCore.Http;
using Lib.helper;
using System;

namespace Lib.auth.provider
{
    /// <summary>
    /// 尝试获取app或者web的token信息
    /// </summary>
    public class AppOrWebAuthDataProvider : IAuthDataProvider
    {
        private readonly HttpContext _context;
        private readonly ITokenEncryption _encryption;
        private readonly AuthOptions _option;

        public AppOrWebAuthDataProvider(
            IHttpContextAccessor accessor,
            ITokenEncryption _encryption,
            AuthOptions option)
        {
            this._context = accessor.HttpContext;
            this._encryption = _encryption;
            this._option = option;

            this._option.Valid();
        }

        public string GetToken()
        {
            var token = this._context.Request.Cookies[this._option.TokenCookieKey] ?? string.Empty;

            return this._encryption.Decrypt(token);
        }

        public void SetToken(string token)
        {
            if (!ValidateHelper.IsPlumpString(token))
                throw new ArgumentException($"{nameof(SetToken)}.token is empty");

            token = this._encryption.Encrypt(token);

            this._context.Response.Cookies.Append(this._option.TokenCookieKey, token, this._option.AsCookieOption());
        }

        public void RemoveToken()
        {
            this._context.Response.Cookies.Delete(this._option.TokenCookieKey, this._option.AsCookieOption());
        }
    }
}
