namespace Lib.auth
{
    /// <summary>
    /// token加密解密
    /// </summary>
    public interface ITokenEncryption
    {
        string Encrypt(string data);
        string Decrypt(string data);
    }
}
