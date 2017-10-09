using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.data
{
    /// <summary>
    /// redis基类，实现序列化和反序列化
    /// </summary>
    public abstract class SerializeBase
    {
        class SerializeWrapper<T>
        {
            public SerializeWrapper() { }

            public T Data { get; set; }
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual byte[] SerializeWithWrapper<T>(T item)
        {
            return this.Serialize(new SerializeWrapper<T>() { Data = item });
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObject"></param>
        /// <returns></returns>
        public virtual T DeserializeWithWrapper<T>(byte[] serializedObject)
        {
            var data = this.Deserialize<SerializeWrapper<T>>(serializedObject);
            if (data != null)
            {
                return data.Data;
            }
            return default(T);
        }

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual byte[] Serialize(object item)
        {
            if (item != null)
            {
                var jsonString = JsonConvert.SerializeObject(item);
                return Encoding.UTF8.GetBytes(jsonString);
            }
            return new byte[] { };
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedObject"></param>
        /// <returns></returns>
        public virtual T Deserialize<T>(byte[] serializedObject)
        {
            if (serializedObject?.Length > 0)
            {
                var jsonString = Encoding.UTF8.GetString(serializedObject);
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            return default(T);
        }
    }

    /// <summary>
    /// 只是继承SerializeBase，没有更多实现
    /// </summary>
    public class SerializeHelper : SerializeBase
    { }
}
