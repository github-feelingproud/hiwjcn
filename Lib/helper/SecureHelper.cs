using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Linq;

namespace Lib.helper
{
    /// <summary>
    /// 安装帮助类
    /// </summary>
    public static class SecureHelper
    {
        private static string BsToStr(byte[] bs) => string.Join(string.Empty, bs.Select(x => x.ToString("x2"))).Replace("-", string.Empty);

        /// <summary>
        /// 获取MD5
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetMD5(string str)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                var bs = Encoding.UTF8.GetBytes(str);
                bs = md5.ComputeHash(bs);
                return BsToStr(bs);
            }
        }

        /// <summary>
        /// 读取文件MD5
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileMD5(string fileName)
        {
            using (var file = new FileStream(fileName, FileMode.Open))
            {
                using (var md5 = new MD5CryptoServiceProvider())
                {
                    var bs = md5.ComputeHash(file);
                    return BsToStr(bs);
                }
            }
        }

        /// <summary>
        /// 获取sha1
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetSHA1(string str)
        {
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                var bs = Encoding.UTF8.GetBytes(str);
                bs = sha1.ComputeHash(bs);
                return BsToStr(bs);
            }
        }

        /// <summary>
        /// SHA256函数
        /// </summary>
        /// /// <param name="str">原始字符串</param>
        /// <returns>SHA256结果</returns>
        public static string GetSHA256(string str)
        {
            using (var Sha256 = new SHA256CryptoServiceProvider())
            {
                var bs = Encoding.UTF8.GetBytes(str);
                bs = Sha256.ComputeHash(bs);
                return BsToStr(bs);
            }
        }

        /// <summary>
        /// 获取hmac md5
        /// </summary>
        /// <param name="str"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string GetHMACMD5(string str, string password)
        {
            using (var hmac_md5 = new HMACMD5())
            {
                hmac_md5.Key = Encoding.UTF8.GetBytes(password);
                var bs = hmac_md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                return BsToStr(bs);
            }
        }

        /// <summary>
        /// 获取hmac sha1
        /// </summary>
        /// <param name="str"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string GetHMACSHA1(string str, string password)
        {
            using (var hmac_sha1 = new HMACSHA1())
            {
                hmac_sha1.Key = Encoding.UTF8.GetBytes(password);
                var bs = hmac_sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
                return BsToStr(bs);
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
        [Obsolete("未测试")]
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
        [Obsolete("未测试")]
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
