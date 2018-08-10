using Lib.helper;
using Lib.threading;
using System.Threading.Tasks;

namespace Lib.auth.provider
{
    public class ScopedUserContext : IScopedUserContext
    {
        private readonly IAuthApi _authApi;
        private readonly IAuthDataProvider _authData;

        private LoginUserInfo _user;
        private readonly object _lock = new object();

        public ScopedUserContext(
            IAuthApi authApi,
            IAuthDataProvider authData)
        {
            this._authApi = authApi;
            this._authData = authData;
        }

        public async Task<LoginUserInfo> GetLoginUserAsync()
        {
            if (this._user == null)
            {
                using (new MonitorLock(this._lock))
                {
                    if (this._user == null)
                    {
                        var token = this._authData.GetToken();
                        if (ValidateHelper.IsPlumpString(token))
                        {
                            var res = await this._authApi.GetLoginUserInfoByTokenAsync(token);
                            if (res.success)
                            {
                                this._user = res.data;
                            }
                        }
                    }
                }
            }

            if (this._user == null)
            {
                this._user = new LoginUserInfo();
            }

            return this._user;
        }

        public void Dispose() { }
    }
}
