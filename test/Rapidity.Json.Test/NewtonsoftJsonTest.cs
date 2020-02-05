using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xunit;

namespace Rapidity.Json.Test
{
    public class NewtonsoftJsonTest
    {
        [Fact]
        public void StackTest()
        {
            var json = "[100,123,45]";
            var list = JsonConvert.DeserializeObject(json, typeof(List<int>));
        }

        [Fact]
        public void WriteTest()
        {
            var list = new Collection<Person>();
            list.Add(new Person()
            {
                Id = Environment.TickCount,
                Name = "测试数据",
                Birthday = DateTime.Now,
                Child = new Person(),
                strField = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                Number = Guid.Empty,
                floadField = float.NegativeInfinity
            });
            var json = JsonConvert.SerializeObject(list);
        }
    }
}
