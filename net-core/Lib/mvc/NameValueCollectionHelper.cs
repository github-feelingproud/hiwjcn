using Lib.cache;
using Lib.core;
using Lib.extension;
using Lib.helper;
using Lib.mvc.user;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Lib.mvc
{
    public static class NameValueCollectionHelper
    {
        /// <summary>
        /// 移除key为null的数据
        /// 移除key和value长度大于32的数据
        /// </summary>
        /// <param name="col"></param>
        /// <param name="nv"></param>
        public static void AddToNameValueCollection(ref NameValueCollection col, NameValueCollection nv)
        {
            foreach (var key in nv.AllKeys)
            {
                if (key == null) { continue; }
                if (key.Length > 32 || nv[key]?.Length > 32) { continue; }

                col[key] = nv[key];
            }
        }
    }
}
