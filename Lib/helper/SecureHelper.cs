using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Lib.helper
{
    /// <summary>
    /// 安装帮助类
    /// </summary>
    public static class SecureHelper
    {
        /// <summary>
        /// 获取MD5
        /// </summary>
        /// <param name="str"></param>
        /// <param name="getShort"></param>
        /// <returns></returns>
        public static string GetMD5(string str, bool getShort = false)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                byte[] bytes_md5_in = Encoding.UTF8.GetBytes(str);
                //把字节数组（byte[]）转换成16进制
                string md5_ret = BitConverter.ToString(md5.ComputeHash(bytes_md5_in));
                md5_ret = ConvertHelper.GetString(md5_ret).Trim().Replace("-", "");
                if (md5_ret.Length != 32) { return string.Empty; }
                if (getShort) { return md5_ret.Substring(8, 16); }
                return md5_ret;
            }
        }

        /// <summary>
        /// SHA256函数
        /// </summary>
        /// /// <param name="str">原始字符串</param>
        /// <returns>SHA256结果</returns>
        public static string SHA256(string str)
        {
            using (var Sha256 = new SHA256Managed())
            {
                byte[] SHA256Data = Encoding.UTF8.GetBytes(str);
                byte[] Result = Sha256.ComputeHash(SHA256Data);
                return Convert.ToBase64String(Result);  //返回长度为44字节的字符串
            }
        }

        public static string GetFileMD5(string fileName)
        {
            FileStream file = null; MD5 md5 = null;
            try
            {
                file = new FileStream(fileName, System.IO.FileMode.Open);
                md5 = new MD5CryptoServiceProvider();
                var bs = md5.ComputeHash(file);
                var sb = new StringBuilder();
                foreach (var b in bs)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (md5 != null) { md5.Dispose(); }
                if (file != null) { file.Dispose(); }
            }
        }

        public static string GetSHA1(string str)
        {
            using (SHA1 sha1 = new SHA1CryptoServiceProvider())
            {
                //把字节数组（byte[]）转换成16进制
                string sha1_ret = BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(str)));
                if (sha1_ret == null || (sha1_ret = sha1_ret.Replace("-", "")).Length != 40) { return ""; }
                return sha1_ret;
            }
        }

        public static string GetHMACMD5(string str, string password)
        {
            using (var hmac_md5 = new HMACMD5())
            {
                hmac_md5.Key = Encoding.UTF8.GetBytes(password);
                //把字节数组（byte[]）转换成16进制
                string hmac_ret = BitConverter.ToString(hmac_md5.ComputeHash(Encoding.UTF8.GetBytes(str)));
                return hmac_ret == null ? string.Empty : hmac_ret.Replace("-", "").ToLower();
            }
        }

        public static string GetHMACSHA1(string str, string password)
        {
            using (var hmac_sha1 = new HMACSHA1())
            {
                hmac_sha1.Key = Encoding.UTF8.GetBytes(password);
                //把字节数组（byte[]）转换成16进制
                string hmac_ret = BitConverter.ToString(hmac_sha1.ComputeHash(Encoding.UTF8.GetBytes(str)));
                return hmac_ret == null ? "" : hmac_ret.Replace("-", "").ToLower();
            }
        }

        //AES密钥向量
        private static readonly byte[] _aeskeys = new byte[] {
            0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD,
            0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF
        };

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="encryptStr">加密字符串</param>
        /// <param name="encryptKey">密钥</param>
        /// <returns></returns>
        public static string AESEncrypt(string encryptStr, string encryptKey)
        {
            if (string.IsNullOrWhiteSpace(encryptStr))
                return string.Empty;

            encryptKey = StringHelper.SubString(encryptKey, 32);
            encryptKey = encryptKey.PadRight(32, ' ');

            //分组加密算法
            using (SymmetricAlgorithm des = Rijndael.Create())
            {
                byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptStr);//得到需要加密的字节数组 
                                                                           //设置密钥及密钥向量
                des.Key = Encoding.UTF8.GetBytes(encryptKey);
                des.IV = _aeskeys;
                byte[] cipherBytes = null;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(inputByteArray, 0, inputByteArray.Length);
                        cs.FlushFinalBlock();
                        cipherBytes = ms.ToArray();//得到加密后的字节数组
                        cs.Close();
                        ms.Close();
                    }
                }
                return Convert.ToBase64String(cipherBytes);
            }
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="decryptStr">解密字符串</param>
        /// <param name="decryptKey">密钥</param>
        /// <returns></returns>
        public static string AESDecrypt(string decryptStr, string decryptKey)
        {
            if (string.IsNullOrWhiteSpace(decryptStr))
                return string.Empty;

            decryptKey = StringHelper.SubString(decryptKey, 32);
            decryptKey = decryptKey.PadRight(32, ' ');

            byte[] cipherText = Convert.FromBase64String(decryptStr);

            using (SymmetricAlgorithm des = Rijndael.Create())
            {
                des.Key = Encoding.UTF8.GetBytes(decryptKey);
                des.IV = _aeskeys;
                byte[] decryptBytes = new byte[cipherText.Length];
                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        cs.Read(decryptBytes, 0, decryptBytes.Length);
                        cs.Close();
                        ms.Close();
                    }
                }
                return Encoding.UTF8.GetString(decryptBytes).Replace("\0", "");//将字符串后尾的'\0'去掉}
            }

        }
    }
}
