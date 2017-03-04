using Hiwjcn.Core.Model.Sys;
using Lib.infrastructure;
using Lib.io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiwjcn.Core.Infrastructure.Common
{
    public interface IUpFileService : IServiceBase<UpFileModel>
    {
        string AddFile(UpFileModel model);

        /// <summary>
        /// 查找文件，给ueditor提供数据
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="filelist"></param>
        /// <param name="filecount"></param>
        void FindFiles(int uid, int start, int size, ref string[] filelist, ref int filecount);

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fid"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        string DeleteFile(int fid, int uid);

        /// <summary>
        /// 上传到七牛，并保存到本地数据库，不重复上传同一个文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="uid"></param>
        /// <param name="file_url"></param>
        /// <param name="file_name"></param>
        /// <returns></returns>
        string UploadFileAfterCheckRepeat(FileInfo file, int uid,
            ref string file_url, ref string file_name, bool DeleteFileAfterUploadToQiniu = true);
    }
}
