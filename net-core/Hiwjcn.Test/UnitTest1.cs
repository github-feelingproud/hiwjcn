using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Hiwjcn.Test
{
    public class UnitTest1
    {
        [Fact]
        public void rdfsafh()
        {
            var collection = new ServiceCollection();

            collection.AddSingleton<User>(new User().MockData());
            collection.AddSingleton<User>(new User().MockData());
            collection.AddSingleton<User>(new User().MockData());

            collection.AddSingleton<Group, Group>();


            var provider = collection.BuildServiceProvider();

            var obj = provider.GetService<Group>();
        }

        public class User : Lib.infrastructure.entity.IMockData<User>
        {
            public User MockData()
            {
                var f = new Bogus.Faker();
                this.IID = f.UniqueIndex;
                this.UID = f.Person.Random.Uuid().ToString();
                this.Name = f.Name.FullName();
                this.Age = f.Random.Int(10, 50);
                this.Address = f.Address.FullAddress();
                this.CreateTime = f.Date.Past();

                return this;
            }

            public virtual long IID { get; set; }

            public virtual string UID { get; set; }

            public virtual string Name { get; set; }

            public virtual int Age { get; set; }

            public virtual string Address { get; set; }

            public virtual DateTime CreateTime { get; set; }
        }

        public class Group
        {
            public Group(IServiceProvider provider)
            {
                var data = provider.GetServices<User>().ToList();
            }
        }

        [Fact]
        public void Test1()
        {
            var user = new User().MockData();
            //print user
        }
    }
}
