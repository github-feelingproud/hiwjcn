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
        
        /// <summary>
        /// 比较两个json结构是否相同
        /// </summary>
        /// <param name="json_1"></param>
        /// <param name="json_2"></param>
        /// <returns></returns>
        public static bool HasSameStructure(string json_1, string json_2)
        {
            var path_list = new List<string>();
            void FindJsonNode(JToken token)
            {
                if (token == null) { return; }
                if (token.Type == JTokenType.Property)
                {
                    path_list.Add(token.Path);
                }
                //find next node
                var children = token.Children();
                foreach (var child in children)
                {
                    FindJsonNode(child);
                }
            }

            FindJsonNode(JToken.Parse(json_1));
            FindJsonNode(JToken.Parse(json_2));

            path_list = path_list.Select(x => ConvertHelper.GetString(x).Trim()).ToList();

            return path_list.Count == path_list.Distinct().Count() * 2;
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
        /// <returns>json</returns>
        public static string DataTableToJson(DataTable dt)
        {
            return JsonConvert.SerializeObject(dt, new DataTableConverter(), TimeFormat());
        }

    }
}
