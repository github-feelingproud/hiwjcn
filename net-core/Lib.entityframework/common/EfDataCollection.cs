using Lib.data;
using Microsoft.EntityFrameworkCore;
using System;

namespace Lib.entityframework
{
    public class EfDataCollection<T> : IDataCollection<T> where T : class
    {
        private readonly Func<DbContext> GetContext;

        public EfDataCollection(Func<DbContext> GetContext)
        {
            this.GetContext = GetContext ?? throw new Exception();
        }

        public int Add()
        {
            using (var db = GetContext())
            {
                db.Set<T>().Add(null);
                return db.SaveChanges();
            }
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
