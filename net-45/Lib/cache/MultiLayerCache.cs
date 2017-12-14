using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.helper;
using Lib.extension;

namespace Lib.cache
{
    public abstract class LayerBase<T>
    {
        public abstract T _GetFromCache(string key);
        public abstract void _SetToCache(string key, T model, TimeSpan expire);

        public Action<string, T, TimeSpan> OnSet { get; set; }
        public Func<string, T> OnGet { get; set; }
    }

    public class DataSourceLayer<T> : LayerBase<T>
    {
        public override T _GetFromCache(string key)
        {
            throw new NotImplementedException("数据源层不需要实现");
        }

        public override void _SetToCache(string key, T model, TimeSpan expire)
        {
            throw new NotImplementedException("数据源层不需要实现");
        }
    }

    public class MultiLayerCache
    {
        public static T Cache<T>(string k, Func<T> dataSource, params LayerBase<T>[] l)
        {
            if (!ValidateHelper.IsPlumpList(l))
            {
                throw new Exception("至少要一个缓存层");
            }
            var layers = new List<LayerBase<T>>(l);
            layers.Add(new DataSourceLayer<T>());
            #region 绑定更新事件
            layers.IterateItems((a, b, index_a, index_b) =>
            {
                //这一层拿不到需要的数据就到下一层拿
                a.OnGet = key =>
                {
                    try
                    {
                        return a._GetFromCache(key);
                    }
                    catch
                    {
                        return b.OnGet(key);
                    }
                };
                //最后一层使用数据源作为数据集
                if (index_b == layers.Count() - 1)
                {
                    //最后一层
                    b.OnGet = key =>
                    {
                        var data = dataSource.Invoke();
                        b.OnSet(key, data, TimeSpan.FromSeconds(10));
                        return data;
                    };
                }

                //下层数据更新触发更新事件，通知上一层更新
                b.OnSet = (key, item, expire) =>
                {
                    a._SetToCache(key, item, expire); a.OnSet(key, item, expire);
                };
            });
            #endregion
            return layers[0].OnGet(k);
        }
    }
}
