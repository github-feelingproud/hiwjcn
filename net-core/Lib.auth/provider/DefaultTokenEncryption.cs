using Lib.extension;
using Lib.helper;
using System;
using System.Linq;

namespace Lib.auth.provider
{
    public class DefaultTokenEncryption : ITokenEncryption
    {
        class EncryptEntry
        {
            public string Token { get; set; }
            public string Salt { get; set; }
            public string Result { get; set; }
        }

        private readonly int TrashLen = 2;

        private string CreateResult(string token, string salt)
        {
            return $"{token}={nameof(DefaultTokenEncryption)}={salt}".ToMD5().ToLower();
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
}
