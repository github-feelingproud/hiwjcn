using Lib.helper;

namespace Lib.auth.provider
{
    public class TokenDesEncryption : ITokenEncryption
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
}
