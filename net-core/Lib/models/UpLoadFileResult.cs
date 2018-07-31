namespace Lib.models
{
    /// <summary>
    /// 上传结果
    /// </summary>
    public class UpLoadFileResult
    {
        public UpLoadFileResult()
        {
            this.SuccessUpload = this.SuccessPreparePath = false;
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
        public long FileSize { get; set; }
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
