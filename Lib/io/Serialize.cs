using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Lib.extension;

namespace Lib.io
{
    public static class Serialize
    {
        /// <summary>
        /// 获取对象序列化的二进制版本
        /// </summary>
        /// <param name="obj">对象实体</param>
        /// <returns>如果对象实体为Null，则返回结果为Null。</returns>
        public static byte[] GetBytes(object obj)
        {
            if (obj == null) { return null; }

            using (var ms = new MemoryStream())
            {
                var formartter = new BinaryFormatter();
                formartter.Serialize(ms, obj);

                ms.Position = 0;
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 从已序列化数据中(byte[])获取对象实体
        /// </summary>
        public static T GetObject<T>(byte[] bs)
        {
            if (bs == null) { return default(T); }
            using (var ms = new MemoryStream(bs))
            {
                var formatter = new BinaryFormatter();

                return (T)formatter.Deserialize(ms);
            }
        }

    }
}
