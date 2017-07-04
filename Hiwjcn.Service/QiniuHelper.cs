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
using Qiniu.Http;

namespace Lib.storage
{
    public static class QiniuExtension
    {
        /// <summary>
        /// 文件存在
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static bool HasFile(this StatResult status)
        {
            return status.Fsize > 0 && ValidateHelper.IsPlumpString(status.Hash) &&
                (!ValidateHelper.IsPlumpString(status?.ResponseInfo?.Error));
        }

        /// <summary>
        /// 有异常就抛出
        /// </summary>
        /// <param name="res"></param>
        public static void ThrowIfException(this ResponseInfo res)
        {
            if (res.isOk()) { return; }
            if (ValidateHelper.IsPlumpString(res.Error))
            {
                throw new Exception(res.Error);
            }
            if (res.isServerError())
            {
                throw new Exception("七牛服务器错误");
            }
        }
    }

    /// <summary>
    /// http://developer.qiniu.com/resource/official.html#sdk
    /// </summary>
    public static class QiniuHelper
    {
        private static readonly string AK = ConfigHelper.Instance.QiniuAccessKey;
        private static readonly string SK = ConfigHelper.Instance.QiniuSecretKey;
        private static readonly string bucket = ConfigHelper.Instance.QiniuBucketName;
        private static readonly string BaseUrl = ConfigHelper.Instance.QiniuBaseUrl;

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
            um.uploadFile(localFile, saveKey, uploadToken, null, (key, resinfo, res) =>
            {
                resinfo.ThrowIfException();
            });
            return GetUrl(saveKey);
        }

        public static string Upload(byte[] bs, string saveKey)
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
            um.uploadData(bs, saveKey, uploadToken, null, (key, resinfo, res) =>
            {
                resinfo.ThrowIfException();
            });
            return GetUrl(saveKey);
        }

        public static string GetUrl(string key)
        {
            return BaseUrl + key;
        }

    }
}
