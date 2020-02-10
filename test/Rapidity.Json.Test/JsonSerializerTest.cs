using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace Rapidity.Json.Test
{
    public class JsonSerializerTest
    {
        [Fact]
        public void ConvertObjectTest()
        {
            var json = "{\"id\":12345,  \"Name\":\"jfkalefjlaj\",\"birthday\":\"2020-02-02 20:10:10\"," +
                "\"Number\":null,\"strField\":\"strFieldstrFieldstrFieldstrField\" , " +
                "\"child\":{\"name\":\"孩子\"},\"EnumField\":\"Machine\"}";
            var reader = new JsonReader(json);
            var serializer = new JsonSerializer();
            var person = serializer.Deserialize<Person>(reader);
        }


        [Fact]
        public void ConvertListTest()
        {
            var json = "[{\"id\":12345,\"Name\":\"jfkalefjlaj\",\"birthday\":\"2020-02-02 20:10:10\",\"Number\":null,\"strField\":\"strFieldstrFieldstrFieldstrField\",\"child\":{\"name\":\"孩子\"}},{\"id\":434343,\"Name\":\"fafea你是\",\"birthday\":\"2020-02-02 20:10:10\",\"Number\":null,\"strField\":\"strFieldstrFieldstrFieldstrField\",\"child\":{}}]";
            var reader = new JsonReader(json);
            var serializer = new JsonSerializer();
            var collection = serializer.Deserialize<IReadOnlyCollection<Person>>(reader);
        }

        [Fact]
        public void ConvertDictionaryTest()
        {
            var json = "{\"name\":\"jluoiuio\",\"id\":\"545465\",\"reamark\":[null] }";
            var reader = new JsonReader(json);
            var serializer = new JsonSerializer();
            var collection = serializer.Deserialize<IDictionary<string, object>>(reader);
        }

        [Fact]
        public void SerializeListTest()
        {
            var list = new Collection<Person>();
            var p1 = new Person()
            {
                Id = Environment.TickCount,
                Name = "测试数据",
                Birthday = DateTime.Now,
                strField = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                Number = Guid.Empty,
                floadField = float.NegativeInfinity
            };
            p1.Child = p1;
            list.Add(p1);
            list.Add(new Person()
            {
                Id = Environment.TickCount,
                Name = "测试数据1212",
                Birthday = DateTime.Now,
                strField = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                floadField = float.NaN,
                dateTimeKinds = new Collection<DateTimeKind?>() { DateTimeKind.Utc, null },
                Child = p1
            });

            var serializer = new JsonSerializer();
            var json2 = serializer.Serialize(list);

            var list2 = serializer.Deserialize<List<Person>>(new JsonReader(json2));
        }

        [Fact]
        public void WriteListTest()
        {
            var list = new List<object>();
            list.Add(list);
            list.Add(1);
            list.Add(new Person());
            var json = new JsonSerializer().Serialize(list);
        }

        [Fact]
        public void WriteDictionaryTest()
        {
            //var dic = new NameValueCollection();
            var dic = new SortedDictionary<string, string>();
            dic.Add("aaaa", "1111");
            dic.Add("bbbb", "efoioweu");
            dic.Add("nnnn", null);
            var json = new JsonSerializer().Serialize(dic);
        }

        [Fact]
        public void ConvertTokenTest()
        {
            var token = new JsonObject();
            token.AddProperty("name", new JsonString("张三"));
            token.AddProperty("id", new JsonNumber(103));
            token.AddProperty("dateTimeKinds", new JsonArray() { new JsonString("Local"), new JsonNull() });

            var serialize = new JsonSerializer();
            var person = serialize.Deserialize<Person>(token);
        }

        [Fact]
        public void WriteTokenTest()
        {
            var token = new JsonObject();
            token.AddProperty("name", new JsonString("张三"));
            token.AddProperty("age", new JsonNumber(103));
            token.AddProperty("dateTimeKinds", new JsonArray() { new JsonString("Local"), new JsonNull() });

            var serialize = new JsonSerializer();
            var json = serialize.Serialize(token);
        }

        [Fact]
        public void WriteObjectTest()
        {
            var obj = new
            {
                name = "aafeaew",
                age = 100,
                Person = new Person(),
                list = new List<Person>() { new Person() },
                Dic = new Dictionary<object, object>() { ["name"] = "fefe", [new Person()] = Guid.NewGuid() }
            };

            dynamic obj2 = new Person();
            obj2.Name = "afaef";
            var serialize = new JsonSerializer();
            var json = serialize.Serialize(obj2);
        }

        [Fact]
        public void WriteStructTest()
        {
            var person = new Person
            {
                Id = 100,
                Name = "aefawef",
                Child = new Person() { Birthday = DateTime.Now },
                dateTimeKinds = new List<DateTimeKind?>()
            };
            var option = new JsonOption
            {
                Indented = true,
                CamelCaseNamed = true,
                IgnoreNullValue = true
            };
            var json = new JsonSerializer(option).Serialize(person);
        }

        [Fact]
        public void ValueTypeTest()
        {

        }

    }
}