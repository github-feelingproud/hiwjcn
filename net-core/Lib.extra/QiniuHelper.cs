using Qiniu.Http;
using Qiniu.IO;
using Qiniu.IO.Model;
using Qiniu.RS;
using Qiniu.RS.Model;
using Qiniu.Util;
using System;
using System.Configuration;
using Lib.extension;

namespace Lib.extra_
{
    /// <summary>
    /// http://developer.qiniu.com/resource/official.html#sdk
    /// </summary>
    public class QiniuHelper
    {
        public readonly string AK;
        public readonly string SK;
        public readonly string Bucket;
        public readonly string BaseUrl;

        public QiniuHelper() : this(
            ConfigurationManager.AppSettings["QiniuAccessKey"],
            ConfigurationManager.AppSettings["QiniuSecretKey"],
            ConfigurationManager.AppSettings["QiniuBucketName"],
            ConfigurationManager.AppSettings["QiniuBaseUrl"])
        {
            //
        }

        public QiniuHelper(string ak, string sk, string bucket, string base_url)
        {
            this.AK = ak ?? throw new ArgumentNullException(nameof(ak));
            this.SK = sk ?? throw new ArgumentNullException(nameof(sk));
            this.Bucket = bucket ?? throw new ArgumentNullException(nameof(bucket));
            this.BaseUrl = (base_url ?? throw new ArgumentNullException(nameof(base_url))).EnsureTrailingSlash();
        }

        /// <summary>
        /// 创建上传token
        /// </summary>
        /// <returns></returns>
        private string CreateUploadToken()
        {
            // 上传策略
            var putPolicy = new PutPolicy();
            // 设置要上传的目标空间
            putPolicy.Scope = this.Bucket;
            putPolicy.SetExpires(3600);
            var mac = new Mac(this.AK, this.SK);
            // 生成上传凭证
            var uploadToken = new Qiniu.Util.Auth(mac).CreateUploadToken(putPolicy.ToJsonString());
            return uploadToken;
        }

        /// <summary>
        /// 获取七牛的文件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public StatResult FindEntry(string key)
        {
            var mac = new Mac(this.AK, this.SK);
            var bm = new BucketManager(mac);
            // 返回结果存储在result中
            var res = bm.Stat(this.Bucket, key).ThrowIfException();
            return res;
        }

        /// <summary>
        /// 删除七牛的文件
        /// </summary>
        /// <param name="key"></param>
        public void Delete(string key)
        {
            var mac = new Mac(this.AK, this.SK);
            var bm = new BucketManager(mac);
            // 返回结果存储在result中
            var res = bm.Delete(this.Bucket, key).ThrowIfException();
        }
        
        /// <summary>
        /// 上传文件到qiniu，返回访问链接
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string Upload(string localFile, string saveKey)
        {
            // 开始上传文件
            var um = new UploadManager();
            var res = um.UploadFile(localFile, saveKey, this.CreateUploadToken()).ThrowIfException();
            return this.GetUrl(saveKey);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="bs"></param>
        /// <param name="saveKey"></param>
        /// <returns></returns>
        public string Upload(byte[] bs, string saveKey)
        {
            // 开始上传文件
            var um = new UploadManager();
            var res = um.UploadData(bs, saveKey, this.CreateUploadToken()).ThrowIfException();
            return this.GetUrl(saveKey);
        }

        /// <summary>
        /// 获取文件地址http://www.domain.com/file-qiniu-key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetUrl(string key) => $"{BaseUrl}{key}";

    }

    public static class QiniuExtension
    {
        /// <summary>
        /// 文件存在
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static bool HasFile(this StatResult status)
        {
            return status.IsOk() &&
                status.Result != null &&
                status.Result.Fsize > 0 &&
                status.Result.Hash?.Length > 0;
        }

        /// <summary>
        /// 是否是200
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public static bool IsOk(this HttpResult res) => res.Code == 200;

        /// <summary>
        /// 有异常就抛出
        /// </summary>
        /// <param name="res"></param>
        public static T ThrowIfException<T>(this T res) where T : HttpResult
        {
            if (!res.IsOk())
            {
                throw new Exception($"七牛服务器错误，返回数据：{res.ToJson()}");
            }
            return res;
        }
    }

}
