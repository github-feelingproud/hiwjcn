using System;

namespace Lib.data
{
    /// <summary>
    /// 统一不同数据库之间的数据集合
    /// 
    /// //这样ef的多次add一次提交就没用了
    /// 
    /// 建一个base repository
    /// ef的repository继承base repository
    /// 
    /// </summary>
    public interface IDataCollection<T>
    {
        int Add();
        int Delete();
        int Update();
        int Query();
    }

    public interface IDataSourceContext
    {
        IDataCollection<T> Set<T>() where T : class;
    }
    
    public class MongoDataCollection<T> : IDataCollection<T>
    {
        public int Add()
        {
            throw new NotImplementedException();
        }

        public int Delete()
        {
            throw new NotImplementedException();
        }

        public int Query()
        {
            throw new NotImplementedException();
        }

        public int Update()
        {
            throw new NotImplementedException();
        }
    }
}
