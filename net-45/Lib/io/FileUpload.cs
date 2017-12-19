using Lib.helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Lib.extension;

namespace Lib.io
{
    /// <summary>
    /// 用来上传文件的类
    /// authour:WJ
    /// </summary>
    public class FileUpload
    {
        #region 属性
        /// <summary>
        /// 文件上传类实例化
        /// </summary>
        public FileUpload()
        {
            MaxSize = 1024 * 1024;
            AllowFileType = new string[] { ".jpg", ".jpeg", ".png", ".gif" };
        }

        /// <summary>
        /// 允许上传的最大尺寸，注意是字节
        /// </summary>
        public int MaxSize { get; set; }

        /// <summary>
        /// 允许的文件格式，带“点”。注意是小写
        /// </summary>
        public string[] AllowFileType { get; set; }
        #endregion

        #region 私有方法
        /// <summary>
        /// 获取一个新的文件模型
        /// </summary>
        /// <returns></returns>
        private UpLoadFileResult PrepareNewFile(HttpPostedFile http_file, string save_path)
        {
            var model = new UpLoadFileResult();
            try
            {
                if (http_file == null || http_file.InputStream == null)
                {
                    model.Info = "文件不存在";
                    return model;
                }
                if (http_file.ContentLength > MaxSize)
                {
                    model.Info = "文件超出大小限制";
                    return model;
                }
                var file_name = http_file.FileName;
                model.OriginName = file_name;
                var file_extesion = IOHelper.GetFileExtension(http_file.ContentType, http_file.FileName).ToLower();
                //////////////////////////////////////////////////
                if (ValidateHelper.IsPlumpList(AllowFileType))
                {
                    //检查文件格式
                    if (AllowFileType.Where(x => x.ToLower() == file_extesion).Count() <= 0)
                    {
                        model.Info = "文件格式不允许";
                        return model;
                    }
                }
                //检查存储路径是否存在，不存在就创建
                var now = DateTime.Now;
                var YEAR = now.Year.ToString();
                var MONTH = now.Month.ToString();
                var DAY = now.Day.ToString();
                var HOUR = now.Hour.ToString();
                //根据年月日小时生成文件夹
                save_path = Path.Combine(save_path, YEAR, MONTH, DAY, HOUR);
                new DirectoryInfo(save_path).CreateIfNotExist();
                //如果文件存在就用guid和随机字符胡乱命名
                do
                {
                    file_name = Com.GetUUID() + file_extesion;
                }
                while (File.Exists(Path.Combine(save_path, file_name)));
                //文件名称（xxx.jpg）
                model.FileName = file_name;
                //文件后缀(.jpg)
                model.FileExtension = file_extesion;
                //文件大小
                model.FileSize = http_file.ContentLength;
                //本地存储文件夹路径
                model.DirectoryPath = save_path;
                //浏览器相对路径
                model.WebPath = string.Join("/", new string[] { YEAR, MONTH, DAY, HOUR }) + "/";
                //本地存储绝对文件路径
                model.FilePath = Path.Combine(save_path, file_name);

                model.SuccessPreparePath = true;
            }
            catch (Exception e)
            {
                model.Info = "准备存储位置发生错误：" + e.Message;
                return model;
            }
            return model;
        }
        #endregion

        #region 提供调用的方法
        /// <summary>
        /// 上传单个文件
        /// </summary>
        /// <param name="http_file"></param>
        /// <param name="save_path"></param>
        /// <returns></returns>
        public UpLoadFileResult UploadSingleFile(HttpPostedFile http_file, string save_path)
        {
            //获取一个新的文件模型
            var model = PrepareNewFile(http_file, save_path);
            if (!model.SuccessPreparePath)
            {
                return model;
            }
            //循环写数据，结束后关闭输入输出流
            using (http_file.InputStream)
            {
                using (var fs = new FileStream(model.FilePath, FileMode.Create, FileAccess.Write))
                {
                    var buffer = new byte[1024 * 1024];
                    int len = 0;
                    while ((len = http_file.InputStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, len);
                    }
                    fs.Flush();
                }
            }

            model.SuccessUpload = true;
            return model;
        }
        /// <summary>
        /// 上传所有文件
        /// </summary>
        /// <returns>返回上传后的文件信息</returns>
        public List<UpLoadFileResult> UploadAllFile(HttpFileCollection http_files, string save_path)
        {
            var filelist = new List<UpLoadFileResult>();

            if (http_files == null || http_files.Count == 0) { return filelist; }

            for (int i = 0; i < http_files.Count; ++i)
            {
                filelist.Add(this.UploadSingleFile(http_files[i], save_path));
            }
            return filelist;
        }
        #endregion

    }

    /// <summary>
    /// 上传结果
    /// </summary>
    public class UpLoadFileResult
    {
        public UpLoadFileResult()
        {
            SuccessUpload = SuccessPreparePath = false;
        }
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 原始名称
        /// </summary>
        public string OriginName { get; set; }
        /// <summary>
        /// 存储的相对路径
        /// </summary>
        public string WebPath { get; set; }
        /// <summary>
        /// 文件的扩展名
        /// </summary>
        public string FileExtension { get; set; }
        /// <summary>
        /// 存储的本地完全路径
        /// </summary>
        public string DirectoryPath { get; set; }
        /// <summary>
        /// DirectoryPath+FileName
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 文件的大小 单位【字节】
        /// </summary>
        public int FileSize { get; set; }
        /// <summary>
        /// 文件的信息
        /// </summary>
        public string Info { get; set; }
        /// <summary>
        /// 是否正确上传
        /// </summary>
        public bool SuccessUpload { get; set; }
        /// <summary>
        /// 是否准备路径成功
        /// </summary>
        public bool SuccessPreparePath { get; set; }
    }
}

