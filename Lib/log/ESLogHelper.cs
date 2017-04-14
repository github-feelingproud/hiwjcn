using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.core;
using Lib.helper;
using Lib.extension;
using Nest;

namespace Lib.log
{
    public static class ESLogHelper
    {
        public static readonly string IndexName = "lib_es_log_index";
        private static readonly ESLogLine temp = new ESLogLine();

        /// <summary>
        /// 搜索日志
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="keyword"></param>
        /// <param name="logger_name"></param>
        /// <param name="page"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public static async Task<PagerData<ESLogLine>> Search(
            DateTime? start, DateTime? end,
            string keyword, string logger_name,
            int page, int pagesize)
        {
            var sd = new SearchDescriptor<ESLogLine>();
            sd = sd.Index(IndexName);

            var query = new QueryContainer();
            if (start != null)
            {
                query &= new DateRangeQuery() { Field = nameof(temp.UpdateTime), GreaterThanOrEqualTo = start.Value };
            }
            if (end != null)
            {
                query &= new DateRangeQuery() { Field = nameof(temp.UpdateTime), LessThan = end.Value };
            }
            if (ValidateHelper.IsPlumpString(keyword))
            {
                query &= new MatchQuery()
                {
                    Field = nameof(temp.Message),
                    Query = keyword,
                    Operator = Operator.Or,
                    MinimumShouldMatch = "100%"
                };
            }
            if (ValidateHelper.IsPlumpString(logger_name))
            {
                query &= new TermQuery() { Field = nameof(temp.LoggerName), Value = logger_name };
            }
            sd = sd.Query(_ => query);

            var sort = new SortDescriptor<ESLogLine>();
            sort = sort.Descending(x => x.UpdateTime);
            sd = sd.Sort(_ => sort);

            var client = new ElasticClient(ElasticsearchClientManager.Instance.DefaultClient);
            var re = await client.SearchAsync<ESLogLine>(_ => sd);
            if (!re.IsValid)
            {
                re.LogError();
                if (re.OriginalException != null)
                {
                    throw re.OriginalException;
                }
                throw new Exception("log搜索错误");
            }
            var data = new PagerData<ESLogLine>();
            data.ItemCount = (int)re.Total;
            data.DataList = re.Hits.Select(x => x.Source).ToList();

            return data;
        }

        public static void ClearExpireData()
        { }
    }
}
