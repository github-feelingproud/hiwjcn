using Lib.helper;
using Lib.io;
using Lib.mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace WebApp.Areas.Admin.Controllers
{
    public class FileController : WebCore.MvcLib.Controller.UserBaseController
    {
        public ActionResult FileAction()
        {
            return Content("");
        }

        /// <summary>
        /// 文件浏览器
        /// </summary>
        /// <returns></returns>
        public ActionResult FileExplorer(string path)
        {
            return RunActionWhenLogin((loginuser)=> 
            {
                string base_url = this.X.BaseUrl;
                string sp = ConvertHelper.GetString(IOHelper.GetSysPathSeparator());
                string base_path = ConvertHelper.GetString(ServerHelper.GetMapPath(this.X.context, "~/static/"));
                if (!base_path.EndsWith(sp)) { base_path += sp; }
                if (!Directory.Exists(base_path)) { return Content("跟路径不存在"); }
                //计算访问路径
                string current_path = path;
                if (ValidateHelper.IsPlumpString(current_path))
                {
                    current_path = string.Join(sp, StringHelper.Split(current_path, '/', '\\'));
                    if (!current_path.EndsWith(sp)) { current_path += sp; }
                    current_path = base_path + current_path;
                    if (!Directory.Exists(current_path)) { current_path = base_path; }
                }
                else
                {
                    current_path = base_path;
                }
                string[] dirs = Directory.GetDirectories(current_path);
                string[] files = Directory.GetFiles(current_path);
                List<FileModel> list = new List<FileModel>();
                if (ValidateHelper.IsPlumpList(dirs))
                {
                    List<DirectoryInfo> existDirs = dirs.Select(x => new DirectoryInfo(x)).Where(x => x.Exists).ToList();
                    list.AddRange(existDirs.Select(x => new FileModel()
                    {
                        FileName = x.Name,
                        FullName = x.FullName,
                        IsDir = true,
                        LastAccessTime = x.LastAccessTime,
                        LastWriteTime = x.LastAccessTime,
                        RelativePath = EncodingHelper.UrlEncode(ConvertHelper.GetString(x.FullName).Replace(base_path, ""))
                    }));
                }
                if (ValidateHelper.IsPlumpList(files))
                {
                    List<FileInfo> existFiles = files.Select(x => new FileInfo(x)).Where(x => x.Exists).ToList();
                    list.AddRange(existFiles.Select(x => new FileModel()
                    {
                        FileName = x.Name,
                        FullName = x.FullName,
                        IsDir = false,
                        LastAccessTime = x.LastAccessTime,
                        LastWriteTime = x.LastAccessTime,
                        Size = x.Length,
                        RelativePath = EncodingHelper.UrlEncode(ConvertHelper.GetString(x.FullName).Replace(base_path, ""))
                    }));
                }
                ViewData["list"] = list;
                //获取上一级
                {
                    //如果路径最后有分隔符（\或者/），获取上一级路径会获取当前路径。所以要去掉最后的分隔符
                    if (current_path.EndsWith(sp))
                    {
                        current_path = current_path.TrimEnd(sp.ToArray());
                        //StringHelper.TrimEnd(current_path, sp);
                    }
                }
                DirectoryInfo parent = Directory.GetParent(current_path);
                if (parent == null || !parent.Exists) { return View(); }
                string parent_fullpath = ConvertHelper.GetString(parent.FullName);
                if (parent_fullpath.Contains(base_path))
                {
                    ViewData["parent"] = EncodingHelper.UrlEncode(parent_fullpath.Replace(base_path, ""));
                }
                return View();
            });
        }

    }
}
