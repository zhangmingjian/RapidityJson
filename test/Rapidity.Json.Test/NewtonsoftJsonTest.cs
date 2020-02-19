using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        public void SerializeTest()
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
            list.Add(null);
            var setting = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var dic = new Dictionary<string, string>()
            {
                ["aaa"] = "aaaa",
                ["nnnn"] = null
            };
            //CamelCasePropertyName
            var json = JsonConvert.SerializeObject(list, setting);
            //JsonPropertyAttribute
        }

        [Fact]
        public void WriterTest()
        {
            var sw = new StringWriter();
            var writer = new JsonTextWriter(sw);
            writer.WriteStartObject();

            writer.WriteComment("//121232324434335 342334534 \n afewegwgwr");
            writer.WriteEndObject();

            var json = sw.ToString();
            var token = JToken.Parse(json);
        }
    }
}
