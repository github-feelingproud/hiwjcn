using System;
using Xunit;

namespace Hiwjcn.Test
{
    public class UnitTest1
    {
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

        [Fact]
        public void Test1()
        {
            var user = new User().MockData();
            //print user
        }
    }
}
