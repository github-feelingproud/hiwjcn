using Lib.core;
using Lib.helper;

namespace Lib.extension
{
    public static class JsonExtension
    {
        /// <summary>
        /// 转为json 
        /// </summary>
        public static string ToJson(this object data) => JsonHelper.ObjectToJson(data);

        /// <summary>
        /// json转为实体
        /// </summary>
        public static T JsonToEntity<T>(this string json, bool throwIfException = true, T deft = default(T))
        {
            try
            {
                return JsonHelper.JsonToEntity<T>(json);
            }
            catch when (!throwIfException)
            {
                return deft;
            }
        }

        /// <summary>
        /// json转为实体，出现异常就返回默认值
        /// </summary>
        public static T JsonToEntityOrDefault<T>(this string json) =>
            json.JsonToEntity<T>(throwIfException: false);

        /// <summary>
        /// 转json，再转实体
        /// </summary>
        public static T CopySelf_<T>([NotNull]this T data) =>
            data.ToJson().JsonToEntity<T>();
    }
}
