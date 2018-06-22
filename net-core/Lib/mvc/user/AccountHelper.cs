using Lib.core;
using Lib.extension;
using Lib.helper;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Lib.mvc.user
{
    /// <summary>
    /// 对token加密
    /// </summary>
    public interface CookieTokenEncryption
    {
        string Encrypt(string data);
        string Decrypt(string data);
    }

    /// <summary>
    /// 可以用
    /// </summary>
    public class DefaultCookieTokenEncryption : CookieTokenEncryption
    {
        class EncryptEntry
        {
            public string Token { get; set; }
            public string Salt { get; set; }
            public string Result { get; set; }
        }

        private readonly int TrashLen = 2;

        public DefaultCookieTokenEncryption()
        {
            //
        }

        private string CreateResult(string token, string salt)
        {
            return $"{token}={nameof(DefaultCookieTokenEncryption)}={salt}".ToMD5().ToLower();
        }

        public string Decrypt(string data)
        {
            try
            {
                if (data.Length <= this.TrashLen * 2) { return string.Empty; }
                //->remove trash
                data = data.Substring(this.TrashLen, data.Length - this.TrashLen * 2);
                //->reverse
                data = data.ToCharArray().Reverse_().AsString();
                //->entity
                var entry = data.Base64ToString().JsonToEntity<EncryptEntry>();
                if (entry.Result != this.CreateResult(entry.Token, entry.Salt))
                {
                    return string.Empty;
                }
                return entry.Token;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string Encrypt(string data)
        {
            var ran = new Random((int)DateTime.Now.Ticks);

            var chars = Com.Range((int)'a', (int)'z').Select(x => (char)x).ToList();
            chars.AddRange(Com.Range((int)'A', (int)'Z').Select(x => (char)x));
            chars.AddRange(new char[] { '+', '-', '*', '/', '=' });

            var salt = ran.Sample(chars, ran.RealNext(3, 6)).AsString();
            var entry = new EncryptEntry()
            {
                Token = data,
                Salt = salt,
            };
            entry.Result = this.CreateResult(entry.Token, entry.Salt);
            //->base64
            data = entry.ToJson().StringToBase64();
            //->reverse
            data = data.ToCharArray().Reverse_().AsString();
            //->add trash
            data = ran.Sample(chars, this.TrashLen).AsString() + data + ran.Sample(chars, this.TrashLen).AsString();

            return data;
        }
    }

    public class CookieTokenDesEncryption : CookieTokenEncryption
    {
        private const string Key = "LiuJineagel=";

        public string Decrypt(string data)
        {
            data = ConvertHelper.GetString(data);
            return SecureHelper.DESDecrypt(data, Key);
        }

        public string Encrypt(string data)
        {
            data = ConvertHelper.GetString(data);
            return SecureHelper.DESEncrypt(data, Key);
        }
    }

    /// <summary>
    /// 登录状态存取
    /// </summary>
    public class LoginStatus
    {
        private readonly CookieTokenEncryption _CookieTokenEncryption;
        //COOKIE
        private readonly string COOKIE_LOGIN_UID;
        //TOKEN
        private readonly string COOKIE_LOGIN_TOKEN;
        //SESSION
        private readonly string LOGIN_USER_SESSION;
        //DOMAIN
        private readonly string COOKIE_DOMAIN;
        //cookie过期的时间
        private readonly int CookieExpiresMinutes;

        public LoginStatus() : this("USER_UID", "USER_TOKEN", "LOGIN_USER_SESSION", ConfigHelper.Instance.CookieDomain)
        { }

        public LoginStatus(string uid, string token, string session,
            string domain = null,
            CookieTokenEncryption _CookieTokenEncryption = null)
        {
            this.COOKIE_DOMAIN = domain;
            var prefix = string.Empty;
            if (!ValidateHelper.IsPlumpString(this.COOKIE_DOMAIN))
            {
                prefix = "_";
            }

            this.COOKIE_LOGIN_UID = prefix + uid;
            this.COOKIE_LOGIN_TOKEN = prefix + token;
            if (this.CookieExpiresMinutes <= 0)
            {
                this.CookieExpiresMinutes = ConfigHelper.Instance.CookieExpiresMinutes;
            }

            this.LOGIN_USER_SESSION = session;

            this._CookieTokenEncryption = _CookieTokenEncryption ?? new DefaultCookieTokenEncryption();
        }

        public string GetCookieUID(HttpContext context)
        {
            return context.Request.Cookies[this.COOKIE_LOGIN_UID] ?? string.Empty;
        }

        public string GetCookieTokenRaw(HttpContext context)
        {
            return context.Request.Cookies[this.COOKIE_LOGIN_TOKEN] ?? string.Empty;
        }

        public string GetCookieToken(HttpContext context)
        {
            var data = this.GetCookieTokenRaw(context);
            if (!ValidateHelper.IsPlumpString(data))
            {
                return string.Empty;
            }

            return this._CookieTokenEncryption.Decrypt(data);
        }

        public void SetUserLogin(HttpContext context, LoginUserInfo loginuser)
        {
            if (loginuser == null) { throw new ArgumentNullException("登陆状态为空"); }
            if (this.CookieExpiresMinutes <= 0) { throw new Exception("cookie过期时间必须大于0，请修改配置"); }

            if (!ValidateHelper.IsPlumpString(loginuser.UserID))
            {
                throw new Exception($"记录登录状态失败，缺少{nameof(loginuser.UserID)}");
            }
            if (!ValidateHelper.IsPlumpString(loginuser.LoginToken))
            {
                throw new Exception($"记录登录状态失败，缺少{nameof(loginuser.LoginToken)}");
            }

            //保存到session
            context.Session.SetObjectAsJson(this.LOGIN_USER_SESSION, loginuser);
            //保存到cookie
            if (this.GetCookieUID(context) != loginuser.UserID)
            {
                context.Response.Cookies.Append(
                    this.COOKIE_LOGIN_UID, loginuser.UserID,
                    new CookieOptions()
                    {
                        Domain = this.COOKIE_DOMAIN,
                        Expires = new DateTimeOffset(DateTime.Now.AddMinutes(this.CookieExpiresMinutes))
                    });
            }
            if (this.GetCookieToken(context) != loginuser.LoginToken)
            {
                var data = this._CookieTokenEncryption.Encrypt(loginuser.LoginToken);

                context.Response.Cookies.Append(
                    this.COOKIE_LOGIN_TOKEN, data,
                    new CookieOptions()
                    {
                        Domain = this.COOKIE_DOMAIN,
                        Expires = new DateTimeOffset(DateTime.Now.AddMinutes(this.CookieExpiresMinutes))
                    });
            }
        }

        public void SetUserLogout(HttpContext context)
        {
            context.Session.RemoveSession(this.LOGIN_USER_SESSION);
            //清空其他cookie操作
            //CookieHelper.RemoveResponseCookies(context, new string[] { COOKIE_LOGIN_UID, COOKIE_LOGIN_TOKEN });
            if (ValidateHelper.IsPlumpString(this.GetCookieTokenRaw(context)))
            {
                context.Response.Cookies.Delete(this.COOKIE_LOGIN_TOKEN,
                    new CookieOptions()
                    {
                        Domain = this.COOKIE_DOMAIN
                    });
            }
            if (ValidateHelper.IsPlumpString(this.GetCookieUID(context)))
            {
                context.Response.Cookies.Delete(this.COOKIE_LOGIN_UID,
                    new CookieOptions()
                    {
                        Domain = this.COOKIE_DOMAIN
                    });
            }
        }
    }
}
