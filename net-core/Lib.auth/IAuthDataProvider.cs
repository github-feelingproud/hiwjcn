namespace Lib.auth
{
    /// <summary>
    /// 获取token和client 信息的渠道
    /// </summary>
    public interface IAuthDataProvider
    {
        string GetToken();

        void SetToken(string token);

        void RemoveToken();
    }
}
