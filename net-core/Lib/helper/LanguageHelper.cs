using Lib.mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lib.helper
{
    public class WordModel
    {
        public virtual string key { get; set; }
        public virtual string value { get; set; }
    }
    public class LangModel
    {
        public virtual string Name { get; set; }
        public virtual string Desc { get; set; }
        public virtual bool Default { get; set; }
        public virtual List<WordModel> Dict { get; set; }
    }

    /// <summary>
    /// 多语言
    /// </summary>
    public static class LanguageHelper
    {
        private static List<LangModel> data = null;

        private static object locker = new object();

        public const string CookieName = "lang";

        public static List<LangModel> LoadAndCacheLanguages()
        {
            if (data == null)
            {
                lock (locker)
                {
                    if (data == null)
                    {
                        string path = ServerHelper.GetMapPath("~/App_Data/Lang/");
                        if (!Directory.Exists(path)) { throw new Exception("语言目录不存在"); }
                        var files = Directory.GetFiles(path, "*.json");

                        data = new List<LangModel>();

                        foreach (var f in files)
                        {
                            var json = File.ReadAllText(f);
                            var m = JsonHelper.JsonToEntity<LangModel>(json);
                            data.Add(m);
                        }

                        if (data.Where(x => x.Default).Count() > 1)
                        {
                            throw new Exception("默认语言最多只能有一个");
                        }
                    }
                }
            }

            return data;
        }
    }
}
