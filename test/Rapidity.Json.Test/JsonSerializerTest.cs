using Rapidity.Json.Test.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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
        public void ConvertConstructorTest()
        {
            var json = "{\"person\":{\"name\":\"test\"}}";
            var model = new JsonSerializer().Deserialize<ConstructorModel>(new JsonReader(json));
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
        public void ConvertArrayTest()
        {
            var json = "[{},{},{}]";
            var reader = new JsonReader(json);
            var serializer = new JsonSerializer(new JsonOption { Indented = true });
            {
                var array = serializer.Deserialize<object[]>(reader);
                Assert.Equal(3, array.Length);
            }

            {
                var arrayToken = new JsonArray();
                arrayToken.Add(10);
                arrayToken.Add(11);
                arrayToken.Add(123);
                arrayToken.Add(109);
                arrayToken.Add(298);
                var arr = serializer.Deserialize<int[]>(arrayToken);
                Assert.Equal(5, arr.Length);

                var objarr = new string[][] { new string[] { "aaaa", "bbbbb\r\naefawe" } };
                var str = serializer.Serialize(objarr);
            }
        }

        [Fact]
        public void ConvertDictionaryTest()
        {
            var json = "{\"name\":\"jluoiuio\",\"id\":\"545465\",\"reamark\":[null] }";
            var serializer = new JsonSerializer();
            var dic1 = serializer.Deserialize<IDictionary<string, object>>(new JsonReader(json));
            var dic2 = serializer.Deserialize<Dictionary<string, object>>(new JsonReader(json));
            var dic3 = serializer.Deserialize<SortedDictionary<string, object>>(new JsonReader(json));
            var dic4 = serializer.Deserialize<SortedList<string, object>>(new JsonReader(json));
            var dic5 = serializer.Deserialize<ConcurrentDictionary<string, object>>(new JsonReader(json));
        }


        [Fact]
        public void KeyPairsValueTest()
        {
            var serializer = new JsonSerializer(new JsonOption { Indented = true });
            var pairs = new KeyValuePair<int, ValueModel>(13454, new ValueModel() { StringValue = "\uffff" });
            var json = serializer.Serialize(pairs);
            var value = serializer.Deserialize<KeyValuePair<int, ValueModel>>(new JsonReader(json));

            var token = JsonObject.Parse(json);
            var value2 = token.To<KeyValuePair<int, ValueModel>>();
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
        public void WriteArrayListTest()
        {
            var list = new List<object>();
            list.Add(DateTime.Now);
            list.Add(1);
            list.Add(true);
            list.Add(new Person());
            var json = new JsonSerializer().Serialize(list);
            var arrayList = new JsonSerializer().Deserialize<ArrayList>(new JsonReader(json));
        }

        [Fact]
        public void WriteDictionaryTest()
        {
            //var dic = new NameValueCollection();
            var dic = new SortedDictionary<string, int>();
            dic.Add("aaaa", 11212);
            dic.Add("bbbb", 2222);
            dic.Add("nnnn", 44444);
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
        public void SerializerTest()
        {
            var json = "{\"date\":\"20200211\",\"stories\":[{\"image_hue\":\"0x41687d\",\"title\":\"为什么美国两党要互相攻讦，不能以国家利益团结在一起？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720273\",\"hint\":\"知乎用户 · 2 分钟阅读\",\"ga_prefix\":\"021116\",\"images\":[\"https:\\/\\/pic3.zhimg.com\\/v2-4868259313b9ef63df75808f51d663ba.jpg\"],\"type\":0,\"id\":9720273},{\"image_hue\":\"0x7db39d\",\"title\":\"为什么有很多人向往去南极旅行？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720257\",\"hint\":\"Pony爸爸 · 6 分钟阅读\",\"ga_prefix\":\"021111\",\"images\":[\"https:\\/\\/pic4.zhimg.com\\/v2-aaa3b9b9e7e5ae4766f1c2f4b36b391b.jpg\"],\"type\":0,\"id\":9720257},{\"image_hue\":\"0x453530\",\"title\":\"你为什么讨厌民国？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720250\",\"hint\":\"孙漾谦 · 2 分钟阅读\",\"ga_prefix\":\"021109\",\"images\":[\"https:\\/\\/pic2.zhimg.com\\/v2-a9baa3d2ebfab4cfd291cdd1a937e979.jpg\"],\"type\":0,\"id\":9720250},{\"image_hue\":\"0xb3937d\",\"title\":\"武汉新型肺炎这轮疫情结束后，离婚率可能会升还是降？对家庭关系带来了哪些影响？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720242\",\"hint\":\"Steve Shi · 3 分钟阅读\",\"ga_prefix\":\"021107\",\"images\":[\"https:\\/\\/pic1.zhimg.com\\/v2-11a6d56909754fc15377e4dbeb4d0c10.jpg\"],\"type\":0,\"id\":9720242},{\"image_hue\":\"0xa2b37d\",\"title\":\"瞎扯 · 如何正确地吐槽\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720238\",\"hint\":\"VOL.2328\",\"ga_prefix\":\"021106\",\"images\":[\"https:\\/\\/pic3.zhimg.com\\/v2-471a2b39664820beaf6c915f42e82d62.jpg\"],\"type\":0,\"id\":9720238}],\"top_stories\":[{\"image_hue\":\"0x736650\",\"hint\":\"作者 \\/ Miss liz\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720198\",\"image\":\"https:\\/\\/pic1.zhimg.com\\/v2-f6a1c61c36fdf2968d3342b985bf1ac0.jpg\",\"title\":\"小事 · 串好的糖葫芦，堆在垃圾箱\",\"ga_prefix\":\"020922\",\"type\":0,\"id\":9720198},{\"image_hue\":\"0x7da2b3\",\"hint\":\"作者 \\/ ChemX\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720115\",\"image\":\"https:\\/\\/pic4.zhimg.com\\/v2-37175280c534cb0a43ab235b9f8fa6fb.jpg\",\"title\":\"你在生活中用过最高端的化学知识是什么？\",\"ga_prefix\":\"020707\",\"type\":0,\"id\":9720115},{\"image_hue\":\"0xb3947d\",\"hint\":\"作者 \\/ 张艾菲\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9719989\",\"image\":\"https:\\/\\/pic3.zhimg.com\\/v2-ff2dfa8f090102b31543cab51adf344a.jpg\",\"title\":\"日本八十年代有哪些好动画？\",\"ga_prefix\":\"020511\",\"type\":0,\"id\":9719989},{\"image_hue\":\"0xb38d7d\",\"hint\":\"作者 \\/ 像少年拉菲迟\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9719812\",\"image\":\"https:\\/\\/pic2.zhimg.com\\/v2-9f5fcb10d94f86a56dfbf4c69d7fdbb9.jpg\",\"title\":\"小事 · 再也回不去的篮球场\",\"ga_prefix\":\"020122\",\"type\":0,\"id\":9719812},{\"image_hue\":\"0x34454a\",\"hint\":\"作者 \\/ Zpuzzle\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9719798\",\"image\":\"https:\\/\\/pic3.zhimg.com\\/v2-7fd42dc941768d7b1b1e7c81962eef0a.jpg\",\"title\":\"为什么会有文学鄙视链的存在，网络文学真的难登大雅之堂吗？\",\"ga_prefix\":\"013116\",\"type\":0,\"id\":9719798}]}";
            var model = JsonParse.To<Rootobject>(json);
        }

        [Fact]
        public void LoopReferenceTest()
        {
            var model = new ClassA
            {
                Name = "test"
            };
            model.ClassB = new ClassB
            {
                Number = 1000,
                ClassA = model
            };
            var option = new JsonOption
            {
                LoopReferenceProcess = Converters.LoopReferenceProcess.Ignore
            };
            var json = new JsonSerializer(option).Serialize(model);
        }

        class ClassA
        {
            public string Name { get; set; }

            public ClassB ClassB { get; set; }
        }

        class ClassB
        {
            public int Number { get; set; }
            public ClassA ClassA { get; set; }
        }

        [Fact]
        public void ValueTypeTest()
        {
            var type = typeof(IList<int>).IsAssignableFrom(typeof(List<int>));
        }
    }
}