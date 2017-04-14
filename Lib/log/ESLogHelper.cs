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
        private static readonly ESLogLine temp = new ESLogLine();

        public static PagerData<ESLogLine> Search(
            DateTime? start, DateTime? end,
            string keyword, string logger_name,
            int page, int pagesize)
        {
            var data = new PagerData<ESLogLine>();
            var client = new ElasticClient(ElasticsearchClientManager.Instance.DefaultClient);

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
                //
            }

            return data;
        }

        public static void ClearExpireData()
        { }
    }
}
