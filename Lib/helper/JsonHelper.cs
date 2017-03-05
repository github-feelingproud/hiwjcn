using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Lib.core;

namespace Lib.helper
{
    public static class JsonHelper
    {
        /// <summary>
        /// json中的时间格式
        /// </summary>
        /// <returns></returns>
        public static IsoDateTimeConverter TimeFormat()
        {
            return new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss" };
        }

        /// <summary>
        /// model转json
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ObjectToJson(object obj)
        {
            if (obj == null) { throw new Exception("null不能被转为json"); }
            return JsonConvert.SerializeObject(obj, TimeFormat());
        }

        private static void JsonParseTest(string json)
        {
            var dom = JObject.Parse(json);
            var mk = dom["io"];
            var props = dom.Properties();
        }

        /// <summary>
        /// json转model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T JsonToEntity<T>(string json)
        {
            if (!ValidateHelper.IsPlumpString(json))
            {
                throw new Exception("json为空");
            }
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// datatable转json
        /// </summary>
        /// <param name="tb">表</param>
        /// <returns>json</returns>
        public static string DataTableToJson(DataTable dt)
        {
            return JsonConvert.SerializeObject(dt, new DataTableConverter(), TimeFormat());
        }

    }
}
