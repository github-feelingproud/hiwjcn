using Nest;

namespace Lib.data.elasticsearch
{
    /// <summary>
    /// 标记为索引model
    /// </summary>
    public interface IElasticSearchIndex : IDBTable { }

    /// <summary>
    /// 用于关键词补全
    /// 分词默认是ik_max_word，可以override
    /// </summary>
    public abstract class CompletionSuggestIndexBase : IElasticSearchIndex
    {
        [Completion(Name = nameof(CompletionSearchTitle), Analyzer = "ik_max_word", SearchAnalyzer = "ik_max_word")]
        public virtual string CompletionSearchTitle { get; set; }
    }
}
