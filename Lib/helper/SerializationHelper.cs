using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;

namespace Lib.helper
{
    /// <summary>
    /// 数据序列化帮助类
    /// </summary>
    public static class SerializationHelper
    {
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="type">对象类型</param>
        /// <param name="filename">文件路径</param>
        /// <returns></returns>
        public static T Load<T>(string filename)
        {
            if (!File.Exists(filename)) { throw new Exception("文件不存在"); }
            var bs = File.ReadAllBytes(filename);
            var str = Encoding.UTF8.GetString(bs);
            return JsonConvert.DeserializeObject<T>(str);
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="filename">文件路径</param>
        public static void Save(object obj, string filename)
        {
            if (File.Exists(filename)) { throw new Exception("文件已存在"); }
            if (obj == null) { throw new Exception("对象为空"); }

            var str = JsonConvert.SerializeObject(obj);

            var bs = Encoding.UTF8.GetBytes(str);

            File.WriteAllBytes(filename, bs);
        }
    }
}
