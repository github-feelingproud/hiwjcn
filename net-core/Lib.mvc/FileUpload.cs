using Lib.extension;
using Lib.helper;
using Lib.models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;

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
        private UpLoadFileResult PrepareNewFile(IFormFile http_file, string save_path)
        {
            var model = new UpLoadFileResult();
            try
            {
                if (http_file.Length > MaxSize)
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
                    if (!AllowFileType.Any(x => x.ToLower() == file_extesion))
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
                model.FileSize = http_file.Length;
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
        public UpLoadFileResult UploadSingleFile(IFormFile http_file, string save_path)
        {
            if (http_file == null) { throw new ArgumentNullException(nameof(http_file)); }
            //获取一个新的文件模型
            var model = PrepareNewFile(http_file, save_path);
            if (!model.SuccessPreparePath)
            {
                return model;
            }
            //循环写数据，结束后关闭输入输出流
            using (var s = http_file.OpenReadStream())
            {
                using (var fs = new FileStream(model.FilePath, FileMode.Create, FileAccess.Write))
                {
                    var buffer = new byte[1024 * 1024];
                    int len = 0;
                    while ((len = s.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, len);
                    }
                    fs.Flush();
                }
            }

            model.SuccessUpload = true;
            return model;
        }
        #endregion
    }
}

