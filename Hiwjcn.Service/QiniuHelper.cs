using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;
using Lib.helper;
using Qiniu.Util;
using Qiniu.Storage;
using Qiniu.Storage.Model;

namespace Hiwjcn.Bll
{
    /// <summary>
    /// http://developer.qiniu.com/resource/official.html#sdk
    /// </summary>
    public static class QiniuHelper
    {
        private static readonly string AK = ConfigHelper.Instance.QiniuAccessKey;
        private static readonly string SK = ConfigHelper.Instance.QiniuSecretKey;
        private static readonly string bucket = ConfigHelper.Instance.QiniuBucketName;

        public static StatResult FindEntry(string key)
        {
            var mac = new Mac(AK, SK);
            var bm = new BucketManager(mac);
            // 返回结果存储在result中
            var res = bm.stat(bucket, key);
            return res;
        }

        public static void Delete(string key)
        {
            var mac = new Mac(AK, SK);
            var bm = new BucketManager(mac);
            // 返回结果存储在result中
            var res = bm.delete(bucket, key);
        }

        /// <summary>
        /// 上传文件到qiniu，返回访问链接
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string Upload(string localFile, string saveKey)
        {
            // 上传策略
            var putPolicy = new PutPolicy();
            // 设置要上传的目标空间
            putPolicy.Scope = bucket;
            var mac = new Mac(AK, SK);
            // 生成上传凭证
            var uploadToken = Qiniu.Util.Auth.createUploadToken(putPolicy, mac);
            // 开始上传文件
            var um = new UploadManager();
            um.uploadFile(localFile, saveKey, uploadToken, null, null);
            return string.Empty;
        }

        public static string GetUrl(string key)
        {
            return ConfigHelper.Instance.QiniuBaseUrl + key;
        }

    }
}
