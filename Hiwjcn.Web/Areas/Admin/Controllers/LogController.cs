using Hiwjcn.Bll.Sys;
using Lib.helper;
using Lib.http;
using Lib.io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace WebApp.Areas.Admin.Controllers
{
    public class LogController : WebCore.MvcLib.Controller.UserBaseController
    {
        public ActionResult ReadLog(string name)
        {
            return RunActionWhenLogin((loginuser) =>
            {
                if (!ValidateHelper.IsPlumpString(name)) { return Content("无效的文件名"); }
                string logPath = ServerHelper.GetMapPath(this.X.context, "~/App_Data/Log/");
                logPath = ConvertHelper.GetString(logPath);
                if (!logPath.EndsWith(IOHelper.GetSysPathSeparator()))
                {
                    logPath += IOHelper.GetSysPathSeparator();
                }
                if (!IOHelper.DirectoryHelper.Exists(logPath))
                {
                    return Content("日志文件夹不存在");
                }
                string logfilePath = logPath + name;
                if (!IOHelper.FileHelper.Exists(logfilePath))
                {
                    return Content("您要访问的文件不存在");
                }
                string content = IOHelper.ReadFileString(logfilePath);
                content = ConvertHelper.GetString(content).Replace("\n", "<br/>");
                ViewData["content"] = content;
                return View();
            });
        }

        public ActionResult LogList()
        {
            return RunActionWhenLogin((loginuser) =>
            {
                string logPath = ServerHelper.GetMapPath(this.X.context, "~/App_Data/Log/");
                logPath = ConvertHelper.GetString(logPath);
                if (!logPath.EndsWith(IOHelper.GetSysPathSeparator()))
                {
                    logPath += IOHelper.GetSysPathSeparator();
                }
                if (!IOHelper.DirectoryHelper.Exists(logPath))
                {
                    return Content("日志文件夹不存在");
                }
                string[] files = Directory.GetFiles(logPath);
                if (!ValidateHelper.IsPlumpList(files))
                {
                    return Content("没有文件");
                }
                files = files.Where(x => x.Contains("LogFile")).ToArray();
                if (!ValidateHelper.IsPlumpList(files))
                {
                    return Content("没有日志");
                }
                FileInfo file = null;
                List<FileModel> list = new List<FileModel>();
                FileModel model = null;
                files.ToList().ForEach(x =>
                {
                    file = new FileInfo(x);
                    if (!file.Exists) { return; }
                    model = new FileModel();
                    model.Size = file.Length;
                    model.FileName = file.Name;
                    model.FullName = file.FullName;
                    list.Add(model);
                });
                ViewData["list"] = list;
                return View();
            });
        }

        public ActionResult SlowReqList()
        {
            return RunActionWhenLogin((loginuser) =>
            {
                var bll = new ReqLogBll();
                var list = bll.GetGroupedList(start: DateTime.Now.AddMonths(-1), count: 50);
                ViewData["list"] = list;
                return View();
            });
        }

    }
}
