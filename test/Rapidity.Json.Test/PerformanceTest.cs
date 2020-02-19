using Rapidity.Json.Test.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace Rapidity.Json.Test
{
    public class PerformanceTest
    {
        private ITestOutputHelper _output;
        private int total = 1000;

        public PerformanceTest(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// JsonReader Parser 性能测试
        /// </summary>
        #region MyRegion
        //[Fact(DisplayName = "JsonReader性能测试")]
        //public void ReadPerformanceTest()
        //{
        //    //var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "large.json");
        //    //var json = File.ReadAllText(path, Encoding.UTF8);
        //    var json = "{\"date\":\"20200211\",\"stories\":[{\"image_hue\":\"0x41687d\",\"title\":\"为什么美国两党要互相攻讦，不能以国家利益团结在一起？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720273\",\"hint\":\"知乎用户 · 2 分钟阅读\",\"ga_prefix\":\"021116\",\"images\":[\"https:\\/\\/pic3.zhimg.com\\/v2-4868259313b9ef63df75808f51d663ba.jpg\"],\"type\":0,\"id\":9720273},{\"image_hue\":\"0x7db39d\",\"title\":\"为什么有很多人向往去南极旅行？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720257\",\"hint\":\"Pony爸爸 · 6 分钟阅读\",\"ga_prefix\":\"021111\",\"images\":[\"https:\\/\\/pic4.zhimg.com\\/v2-aaa3b9b9e7e5ae4766f1c2f4b36b391b.jpg\"],\"type\":0,\"id\":9720257},{\"image_hue\":\"0x453530\",\"title\":\"你为什么讨厌民国？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720250\",\"hint\":\"孙漾谦 · 2 分钟阅读\",\"ga_prefix\":\"021109\",\"images\":[\"https:\\/\\/pic2.zhimg.com\\/v2-a9baa3d2ebfab4cfd291cdd1a937e979.jpg\"],\"type\":0,\"id\":9720250},{\"image_hue\":\"0xb3937d\",\"title\":\"武汉新型肺炎这轮疫情结束后，离婚率可能会升还是降？对家庭关系带来了哪些影响？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720242\",\"hint\":\"Steve Shi · 3 分钟阅读\",\"ga_prefix\":\"021107\",\"images\":[\"https:\\/\\/pic1.zhimg.com\\/v2-11a6d56909754fc15377e4dbeb4d0c10.jpg\"],\"type\":0,\"id\":9720242},{\"image_hue\":\"0xa2b37d\",\"title\":\"瞎扯 · 如何正确地吐槽\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720238\",\"hint\":\"VOL.2328\",\"ga_prefix\":\"021106\",\"images\":[\"https:\\/\\/pic3.zhimg.com\\/v2-471a2b39664820beaf6c915f42e82d62.jpg\"],\"type\":0,\"id\":9720238}],\"top_stories\":[{\"image_hue\":\"0x736650\",\"hint\":\"作者 \\/ Miss liz\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720198\",\"image\":\"https:\\/\\/pic1.zhimg.com\\/v2-f6a1c61c36fdf2968d3342b985bf1ac0.jpg\",\"title\":\"小事 · 串好的糖葫芦，堆在垃圾箱\",\"ga_prefix\":\"020922\",\"type\":0,\"id\":9720198},{\"image_hue\":\"0x7da2b3\",\"hint\":\"作者 \\/ ChemX\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720115\",\"image\":\"https:\\/\\/pic4.zhimg.com\\/v2-37175280c534cb0a43ab235b9f8fa6fb.jpg\",\"title\":\"你在生活中用过最高端的化学知识是什么？\",\"ga_prefix\":\"020707\",\"type\":0,\"id\":9720115},{\"image_hue\":\"0xb3947d\",\"hint\":\"作者 \\/ 张艾菲\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9719989\",\"image\":\"https:\\/\\/pic3.zhimg.com\\/v2-ff2dfa8f090102b31543cab51adf344a.jpg\",\"title\":\"日本八十年代有哪些好动画？\",\"ga_prefix\":\"020511\",\"type\":0,\"id\":9719989},{\"image_hue\":\"0xb38d7d\",\"hint\":\"作者 \\/ 像少年拉菲迟\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9719812\",\"image\":\"https:\\/\\/pic2.zhimg.com\\/v2-9f5fcb10d94f86a56dfbf4c69d7fdbb9.jpg\",\"title\":\"小事 · 再也回不去的篮球场\",\"ga_prefix\":\"020122\",\"type\":0,\"id\":9719812},{\"image_hue\":\"0x34454a\",\"hint\":\"作者 \\/ Zpuzzle\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9719798\",\"image\":\"https:\\/\\/pic3.zhimg.com\\/v2-7fd42dc941768d7b1b1e7c81962eef0a.jpg\",\"title\":\"为什么会有文学鄙视链的存在，网络文学真的难登大雅之堂吗？\",\"ga_prefix\":\"013116\",\"type\":0,\"id\":9719798}]}";
        //    int total = 100;
        //    //jsonreader
        //    {
        //        _output.WriteLine("==Rapidity.Json================");
        //        var watch = Stopwatch.StartNew();
        //        for (int i = 1; i <= total; i++)
        //        {
        //            int count = 0;
        //            using (var read = new JsonReader(json))
        //            {
        //                while (read.Read())
        //                {
        //                    count++;
        //                }
        //            }
        //        }
        //        watch.Stop();
        //        _output.WriteLine($"Rapidity.Json读{total}次用时：{watch.ElapsedMilliseconds}ms");
        //    }

        //    {
        //        var watch = Stopwatch.StartNew();
        //        for (int i = 1; i <= total; i++)
        //        {
        //            var token = JsonToken.Parse(json);
        //        }
        //        watch.Stop();
        //        _output.WriteLine($"Rapidity.Json读+解析{total}次用时：{watch.ElapsedMilliseconds}ms");
        //    }

        //    //newtonsoft.jsonreader
        //    {
        //        _output.WriteLine("==Newtonsoft.Json================");
        //        var watch = Stopwatch.StartNew();
        //        for (int i = 1; i <= total; i++)
        //        {
        //            int count = 0;
        //            using (var read = new Newtonsoft.Json.JsonTextReader(new StringReader(json)))
        //            {
        //                while (read.Read())
        //                {
        //                    count++;
        //                }
        //            }
        //        }
        //        watch.Stop();
        //        _output.WriteLine($"Newtonsoft.Json读{total}次用时：{watch.ElapsedMilliseconds}ms");
        //    }

        //    {
        //        var watch = Stopwatch.StartNew();
        //        for (int i = 1; i <= total; i++)
        //        {
        //            var token = Newtonsoft.Json.Linq.JToken.Parse(json);
        //        }
        //        watch.Stop();
        //        _output.WriteLine($"Newtonsoft.Json读+解析{total}次用时：{watch.ElapsedMilliseconds}ms");

        //    }

        //    //microsoft Json
        //    {
        //        _output.WriteLine("==System.Text.Json================");
        //        var bytes = Encoding.UTF8.GetBytes(json);
        //        var memory = new ReadOnlySpan<byte>(bytes);
        //        var watch = Stopwatch.StartNew();
        //        for (int i = 1; i <= total; i++)
        //        {
        //            var read = new System.Text.Json.Utf8JsonReader(memory);
        //            int count = 0;
        //            while (read.Read())
        //            {
        //                count++;
        //            }
        //        }
        //        watch.Stop();
        //        _output.WriteLine($"System.Text.Json读{total}次用时：{watch.ElapsedMilliseconds}ms");
        //    }

        //    {
        //        var bytes = Encoding.UTF8.GetBytes(json);
        //        var watch = Stopwatch.StartNew();
        //        for (int i = 1; i <= total; i++)
        //        {
        //            var doc = System.Text.Json.JsonDocument.Parse(new ReadOnlyMemory<byte>(bytes));
        //        }
        //        watch.Stop();
        //        _output.WriteLine($"System.Text.Json读+解析{total}次用时：{watch.ElapsedMilliseconds}ms");
        //    }
        //}

        //[Fact(DisplayName = "反序列化性能测试")]
        //public void JsonDeserializerTest()
        //{
        //    int total = 100;
        //    var json = "{\"date\":\"20200211\",\"stories\":[{\"image_hue\":\"0x41687d\",\"title\":\"为什么美国两党要互相攻讦，不能以国家利益团结在一起？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720273\",\"hint\":\"知乎用户 · 2 分钟阅读\",\"ga_prefix\":\"021116\",\"images\":[\"https:\\/\\/pic3.zhimg.com\\/v2-4868259313b9ef63df75808f51d663ba.jpg\"],\"type\":0,\"id\":9720273},{\"image_hue\":\"0x7db39d\",\"title\":\"为什么有很多人向往去南极旅行？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720257\",\"hint\":\"Pony爸爸 · 6 分钟阅读\",\"ga_prefix\":\"021111\",\"images\":[\"https:\\/\\/pic4.zhimg.com\\/v2-aaa3b9b9e7e5ae4766f1c2f4b36b391b.jpg\"],\"type\":0,\"id\":9720257},{\"image_hue\":\"0x453530\",\"title\":\"你为什么讨厌民国？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720250\",\"hint\":\"孙漾谦 · 2 分钟阅读\",\"ga_prefix\":\"021109\",\"images\":[\"https:\\/\\/pic2.zhimg.com\\/v2-a9baa3d2ebfab4cfd291cdd1a937e979.jpg\"],\"type\":0,\"id\":9720250},{\"image_hue\":\"0xb3937d\",\"title\":\"武汉新型肺炎这轮疫情结束后，离婚率可能会升还是降？对家庭关系带来了哪些影响？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720242\",\"hint\":\"Steve Shi · 3 分钟阅读\",\"ga_prefix\":\"021107\",\"images\":[\"https:\\/\\/pic1.zhimg.com\\/v2-11a6d56909754fc15377e4dbeb4d0c10.jpg\"],\"type\":0,\"id\":9720242},{\"image_hue\":\"0xa2b37d\",\"title\":\"瞎扯 · 如何正确地吐槽\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720238\",\"hint\":\"VOL.2328\",\"ga_prefix\":\"021106\",\"images\":[\"https:\\/\\/pic3.zhimg.com\\/v2-471a2b39664820beaf6c915f42e82d62.jpg\"],\"type\":0,\"id\":9720238}],\"top_stories\":[{\"image_hue\":\"0x736650\",\"hint\":\"作者 \\/ Miss liz\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720198\",\"image\":\"https:\\/\\/pic1.zhimg.com\\/v2-f6a1c61c36fdf2968d3342b985bf1ac0.jpg\",\"title\":\"小事 · 串好的糖葫芦，堆在垃圾箱\",\"ga_prefix\":\"020922\",\"type\":0,\"id\":9720198},{\"image_hue\":\"0x7da2b3\",\"hint\":\"作者 \\/ ChemX\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720115\",\"image\":\"https:\\/\\/pic4.zhimg.com\\/v2-37175280c534cb0a43ab235b9f8fa6fb.jpg\",\"title\":\"你在生活中用过最高端的化学知识是什么？\",\"ga_prefix\":\"020707\",\"type\":0,\"id\":9720115},{\"image_hue\":\"0xb3947d\",\"hint\":\"作者 \\/ 张艾菲\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9719989\",\"image\":\"https:\\/\\/pic3.zhimg.com\\/v2-ff2dfa8f090102b31543cab51adf344a.jpg\",\"title\":\"日本八十年代有哪些好动画？\",\"ga_prefix\":\"020511\",\"type\":0,\"id\":9719989},{\"image_hue\":\"0xb38d7d\",\"hint\":\"作者 \\/ 像少年拉菲迟\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9719812\",\"image\":\"https:\\/\\/pic2.zhimg.com\\/v2-9f5fcb10d94f86a56dfbf4c69d7fdbb9.jpg\",\"title\":\"小事 · 再也回不去的篮球场\",\"ga_prefix\":\"020122\",\"type\":0,\"id\":9719812},{\"image_hue\":\"0x34454a\",\"hint\":\"作者 \\/ Zpuzzle\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9719798\",\"image\":\"https:\\/\\/pic3.zhimg.com\\/v2-7fd42dc941768d7b1b1e7c81962eef0a.jpg\",\"title\":\"为什么会有文学鄙视链的存在，网络文学真的难登大雅之堂吗？\",\"ga_prefix\":\"013116\",\"type\":0,\"id\":9719798}]}";
        //    {
        //        var watch = Stopwatch.StartNew();
        //        for (int i = 0; i < total; i++)
        //        {
        //            var model = JsonParse.To<Rootobject>(json);
        //        }
        //        watch.Stop();
        //        _output.WriteLine($"Rapidity.Json反序列化{total}次，用时：{watch.ElapsedMilliseconds}ms");
        //    }

        //    {
        //        var watch = Stopwatch.StartNew();
        //        for (int i = 0; i < total; i++)
        //        {
        //            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Rootobject>(json);
        //        }
        //        watch.Stop();
        //        _output.WriteLine($"Newtonsoft.Json反序列化{total}次，用时：{watch.ElapsedMilliseconds}ms");
        //    }

        //    {
        //        var watch = Stopwatch.StartNew();
        //        for (int i = 0; i < total; i++)
        //        {
        //            var model = System.Text.Json.JsonSerializer.Deserialize<Rootobject>(json);
        //        }
        //        watch.Stop();
        //        _output.WriteLine($"System.Text.Json反序列化{total}次，用时：{watch.ElapsedMilliseconds}ms");
        //    }
        //}

        //[Fact(DisplayName = "序列化性能测试")]
        //public void JsonSerializerTest()
        //{
        //    int total = 100;
        //    var json = "{\"date\":\"20200211\",\"stories\":[{\"image_hue\":\"0x41687d\",\"title\":\"为什么美国两党要互相攻讦，不能以国家利益团结在一起？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720273\",\"hint\":\"知乎用户 · 2 分钟阅读\",\"ga_prefix\":\"021116\",\"images\":[\"https:\\/\\/pic3.zhimg.com\\/v2-4868259313b9ef63df75808f51d663ba.jpg\"],\"type\":0,\"id\":9720273},{\"image_hue\":\"0x7db39d\",\"title\":\"为什么有很多人向往去南极旅行？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720257\",\"hint\":\"Pony爸爸 · 6 分钟阅读\",\"ga_prefix\":\"021111\",\"images\":[\"https:\\/\\/pic4.zhimg.com\\/v2-aaa3b9b9e7e5ae4766f1c2f4b36b391b.jpg\"],\"type\":0,\"id\":9720257},{\"image_hue\":\"0x453530\",\"title\":\"你为什么讨厌民国？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720250\",\"hint\":\"孙漾谦 · 2 分钟阅读\",\"ga_prefix\":\"021109\",\"images\":[\"https:\\/\\/pic2.zhimg.com\\/v2-a9baa3d2ebfab4cfd291cdd1a937e979.jpg\"],\"type\":0,\"id\":9720250},{\"image_hue\":\"0xb3937d\",\"title\":\"武汉新型肺炎这轮疫情结束后，离婚率可能会升还是降？对家庭关系带来了哪些影响？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720242\",\"hint\":\"Steve Shi · 3 分钟阅读\",\"ga_prefix\":\"021107\",\"images\":[\"https:\\/\\/pic1.zhimg.com\\/v2-11a6d56909754fc15377e4dbeb4d0c10.jpg\"],\"type\":0,\"id\":9720242},{\"image_hue\":\"0xa2b37d\",\"title\":\"瞎扯 · 如何正确地吐槽\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720238\",\"hint\":\"VOL.2328\",\"ga_prefix\":\"021106\",\"images\":[\"https:\\/\\/pic3.zhimg.com\\/v2-471a2b39664820beaf6c915f42e82d62.jpg\"],\"type\":0,\"id\":9720238}],\"top_stories\":[{\"image_hue\":\"0x736650\",\"hint\":\"作者 \\/ Miss liz\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720198\",\"image\":\"https:\\/\\/pic1.zhimg.com\\/v2-f6a1c61c36fdf2968d3342b985bf1ac0.jpg\",\"title\":\"小事 · 串好的糖葫芦，堆在垃圾箱\",\"ga_prefix\":\"020922\",\"type\":0,\"id\":9720198},{\"image_hue\":\"0x7da2b3\",\"hint\":\"作者 \\/ ChemX\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720115\",\"image\":\"https:\\/\\/pic4.zhimg.com\\/v2-37175280c534cb0a43ab235b9f8fa6fb.jpg\",\"title\":\"你在生活中用过最高端的化学知识是什么？\",\"ga_prefix\":\"020707\",\"type\":0,\"id\":9720115},{\"image_hue\":\"0xb3947d\",\"hint\":\"作者 \\/ 张艾菲\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9719989\",\"image\":\"https:\\/\\/pic3.zhimg.com\\/v2-ff2dfa8f090102b31543cab51adf344a.jpg\",\"title\":\"日本八十年代有哪些好动画？\",\"ga_prefix\":\"020511\",\"type\":0,\"id\":9719989},{\"image_hue\":\"0xb38d7d\",\"hint\":\"作者 \\/ 像少年拉菲迟\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9719812\",\"image\":\"https:\\/\\/pic2.zhimg.com\\/v2-9f5fcb10d94f86a56dfbf4c69d7fdbb9.jpg\",\"title\":\"小事 · 再也回不去的篮球场\",\"ga_prefix\":\"020122\",\"type\":0,\"id\":9719812},{\"image_hue\":\"0x34454a\",\"hint\":\"作者 \\/ Zpuzzle\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9719798\",\"image\":\"https:\\/\\/pic3.zhimg.com\\/v2-7fd42dc941768d7b1b1e7c81962eef0a.jpg\",\"title\":\"为什么会有文学鄙视链的存在，网络文学真的难登大雅之堂吗？\",\"ga_prefix\":\"013116\",\"type\":0,\"id\":9719798}]}";
        //    var model = JsonParse.To<Rootobject>(json);
        //    {
        //        var watch = Stopwatch.StartNew();
        //        for (int i = 0; i < total; i++)
        //        {
        //            var resultJson = JsonParse.ToJson(model);
        //        }
        //        watch.Stop();
        //        _output.WriteLine($"Rapidity.Json序列化{total}次，用时：{watch.ElapsedMilliseconds}ms");
        //    }

        //    {
        //        var watch = Stopwatch.StartNew();
        //        for (int i = 0; i < total; i++)
        //        {
        //            var resultJson = Newtonsoft.Json.JsonConvert.SerializeObject(model);
        //        }
        //        watch.Stop();
        //        _output.WriteLine($"Newtonsoft.Json序列化{total}次，用时：{watch.ElapsedMilliseconds}ms");
        //    }
        //    {
        //        var watch = Stopwatch.StartNew();
        //        for (int i = 0; i < total; i++)
        //        {
        //            var resultJson = System.Text.Json.JsonSerializer.Serialize(model);
        //        }
        //        watch.Stop();
        //        _output.WriteLine($"System.Text.Json序列化{total}次，用时：{watch.ElapsedMilliseconds}ms");
        //    }

        //}
        #endregion

        #region 分开测试

        [Fact(DisplayName = "Rapidity.Json.Reader测试")]
        public void RapidityJsonReadTest()
        {
            var json = GetJson();
            //jsonreader
            {
                var writer = CreateWriter("JsonReader性能测试");
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < total; i++)
                {
                    using (var read = new JsonReader(json))
                    {
                        while (read.Read())
                        {
                        }
                    }
                }
                watch.Stop();
                var message = $"Rapidity.Json读{total}次用时：{watch.ElapsedMilliseconds}ms";
                WriteLine(writer, message);
            }
        }

        [Fact(DisplayName = "Newtonsoft.Json.Reader测试")]
        public void NewtonsoftJsonReadTest()
        {
            var json = GetJson();
            //jsonreader
            {
                var writer = CreateWriter("JsonReader性能测试");
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < total; i++)
                {
                    using (var read = new Newtonsoft.Json.JsonTextReader(new StringReader(json)))
                    {
                        while (read.Read())
                        {
                        }
                    }
                }
                watch.Stop();
                WriteLine(writer, $"Newtonsoft.Json读{total}次用时：{watch.ElapsedMilliseconds}ms");
            }
        }

        [Fact(DisplayName = "System.Text.Reader测试")]
        public void SystemTextJsonReadTest()
        {
            var json = GetJson();
            //jsonreader
            {
                var writer = CreateWriter("JsonReader性能测试");
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < total; i++)
                {
                    var bytes = Encoding.UTF8.GetBytes(json);
                    var memory = new ReadOnlySpan<byte>(bytes);
                    var read = new System.Text.Json.Utf8JsonReader(memory);
                    while (read.Read())
                    {
                    }
                }
                watch.Stop();
                WriteLine(writer, $"System.Text.Json读{total}次用时：{watch.ElapsedMilliseconds}ms");
            }
        }

        [Fact(DisplayName = "Rapidity.Json反序列化测试")]
        public void RapidityJsonDeserializerTest()
        {
            var json = GetJson();
            {
                var writer = CreateWriter("反序列化性能测试");
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < total; i++)
                {
                    var model = JsonParse.To<Rootobject>(json);
                }
                watch.Stop();
                WriteLine(writer, $"Rapidity.Json反序列化{total}次，用时：{watch.ElapsedMilliseconds}ms");
            }
        }

        [Fact(DisplayName = "Newtonsoft.Json反序列化测试")]
        public void NewtonsoftJsonDeserializerTest()
        {
            var json = GetJson();
            {
                var writer = CreateWriter("反序列化性能测试");
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < total; i++)
                {
                    var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Rootobject>(json);
                }
                watch.Stop();
                WriteLine(writer, $"Newtonsoft.Json反序列化{total}次，用时：{watch.ElapsedMilliseconds}ms");
            }

            {
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < total; i++)
                {
                    var model = Newtonsoft.Json.JsonConvert.DeserializeObject<Rootobject>(json);
                }
                watch.Stop();
                _output.WriteLine($"Newtonsoft.Json反序列化{total}次，用时：{watch.ElapsedMilliseconds}ms");
            }

            {
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < total; i++)
                {
                    var model = System.Text.Json.JsonSerializer.Deserialize<Rootobject>(json);
                }
                watch.Stop();
                _output.WriteLine($"System.Text.Json反序列化{total}次，用时：{watch.ElapsedMilliseconds}ms");
            }
        }

        [Fact(DisplayName = "System.Text.Json反序列化测试")]
        public void SystemTextJsonDeserializerTest()
        {
            var json = GetJson();
            {
                var writer = CreateWriter("反序列化性能测试");
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < total; i++)
                {
                    var model = System.Text.Json.JsonSerializer.Deserialize<Rootobject>(json);
                }
                watch.Stop();
                WriteLine(writer, $"System.Text.Json反序列化{total}次，用时：{watch.ElapsedMilliseconds}ms");
            }
        }

        [Fact(DisplayName = "Rapidity.Json序列化测试")]
        public void RapidityJsonSerializerTest()
        {
            var json = GetJson();
            var model = JsonParse.To<Rootobject>(json);
            {
                var writer = CreateWriter("序列化性能测试");
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < total; i++)
                {
                    var resultJson = JsonParse.ToJson(model);
                }
                watch.Stop();
                WriteLine(writer, $"Rapidity.Json序列化{total}次，用时：{watch.ElapsedMilliseconds}ms");
            }

            {
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < total; i++)
                {
                    var resultJson = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                }
                watch.Stop();
                _output.WriteLine($"Newtonsoft.Json序列化{total}次，用时：{watch.ElapsedMilliseconds}ms");
            }
            {
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < total; i++)
                {
                    var resultJson = System.Text.Json.JsonSerializer.Serialize(model);
                }
                watch.Stop();
                _output.WriteLine($"System.Text.Json序列化{total}次，用时：{watch.ElapsedMilliseconds}ms");
            }
        }

        [Fact(DisplayName = "Newtonsoft.Json序列化测试")]
        public void NewtonsoftJsonSerializerTest()
        {
            var json = GetJson();
            var model = JsonParse.To<Rootobject>(json);
            {
                var writer = CreateWriter("序列化性能测试");
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < total; i++)
                {
                    var resultJson = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                }
                watch.Stop();
                WriteLine(writer, $"Newtonsoft.Json序列化{total}次，用时：{watch.ElapsedMilliseconds}ms");
            }
        }

        [Fact(DisplayName = "System.Text.Json序列化测试")]
        public void SystemTextJsonSerializerTest()
        {
            var json = GetJson();
            var model = JsonParse.To<Rootobject>(json);

            {
                var writer = CreateWriter("序列化性能测试");
                var watch = Stopwatch.StartNew();
                for (int i = 0; i < total; i++)
                {
                    var resultJson = System.Text.Json.JsonSerializer.Serialize(model);
                }
                watch.Stop();
                WriteLine(writer, $"System.Text.Json序列化{total}次，用时：{watch.ElapsedMilliseconds}ms");
            }
        }

        #endregion

        private string GetJson()
        {
            //var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "large.json");
            //var json = File.ReadAllText(path, Encoding.UTF8);
            var json = "{\"date\":\"20200211\"  , \"stories\":[{\"image_hue\":\"0x41687d\",\"title\":\"为什么美国两党要互相攻讦，不能以国家利益团结在一起？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720273\",\"hint\":\"知乎用户 · 2 分钟阅读\",\"ga_prefix\":\"021116\",\"images\":[\"https:\\/\\/pic3.zhimg.com\\/v2-4868259313b9ef63df75808f51d663ba.jpg\"],\"type\":0,\"id\":9720273},{\"image_hue\":\"0x7db39d\",\"title\":\"为什么有很多人向往去南极旅行？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720257\",\"hint\":\"Pony爸爸 · 6 分钟阅读\",\"ga_prefix\":\"021111\",\"images\":[\"https:\\/\\/pic4.zhimg.com\\/v2-aaa3b9b9e7e5ae4766f1c2f4b36b391b.jpg\"],\"type\":0,\"id\":9720257},{\"image_hue\":\"0x453530\",\"title\":\"你为什么讨厌民国？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720250\",\"hint\":\"孙漾谦 · 2 分钟阅读\",\"ga_prefix\":\"021109\",\"images\":[\"https:\\/\\/pic2.zhimg.com\\/v2-a9baa3d2ebfab4cfd291cdd1a937e979.jpg\"],\"type\":0,\"id\":9720250},{\"image_hue\":\"0xb3937d\",\"title\":\"武汉新型肺炎这轮疫情结束后，离婚率可能会升还是降？对家庭关系带来了哪些影响？\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720242\",\"hint\":\"Steve Shi · 3 分钟阅读\",\"ga_prefix\":\"021107\",\"images\":[\"https:\\/\\/pic1.zhimg.com\\/v2-11a6d56909754fc15377e4dbeb4d0c10.jpg\"],\"type\":0,\"id\":9720242},{\"image_hue\":\"0xa2b37d\",\"title\":\"瞎扯 · 如何正确地吐槽\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720238\",\"hint\":\"VOL.2328\",\"ga_prefix\":\"021106\",\"images\":[\"https:\\/\\/pic3.zhimg.com\\/v2-471a2b39664820beaf6c915f42e82d62.jpg\"],\"type\":0,\"id\":9720238}],\"top_stories\":[{\"image_hue\":\"0x736650\",\"hint\":\"作者 \\/ Miss liz\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720198\",\"image\":\"https:\\/\\/pic1.zhimg.com\\/v2-f6a1c61c36fdf2968d3342b985bf1ac0.jpg\",\"title\":\"小事 · 串好的糖葫芦，堆在垃圾箱\",\"ga_prefix\":\"020922\",\"type\":0,\"id\":9720198},{\"image_hue\":\"0x7da2b3\",\"hint\":\"作者 \\/ ChemX\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9720115\",\"image\":\"https:\\/\\/pic4.zhimg.com\\/v2-37175280c534cb0a43ab235b9f8fa6fb.jpg\",\"title\":\"你在生活中用过最高端的化学知识是什么？\",\"ga_prefix\":\"020707\",\"type\":0,\"id\":9720115},{\"image_hue\":\"0xb3947d\",\"hint\":\"作者 \\/ 张艾菲\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9719989\",\"image\":\"https:\\/\\/pic3.zhimg.com\\/v2-ff2dfa8f090102b31543cab51adf344a.jpg\",\"title\":\"日本八十年代有哪些好动画？\",\"ga_prefix\":\"020511\",\"type\":0,\"id\":9719989},{\"image_hue\":\"0xb38d7d\",\"hint\":\"作者 \\/ 像少年拉菲迟\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9719812\",\"image\":\"https:\\/\\/pic2.zhimg.com\\/v2-9f5fcb10d94f86a56dfbf4c69d7fdbb9.jpg\",\"title\":\"小事 · 再也回不去的篮球场\",\"ga_prefix\":\"020122\",\"type\":0,\"id\":9719812},{\"image_hue\":\"0x34454a\",\"hint\":\"作者 \\/ Zpuzzle\",\"url\":\"https:\\/\\/daily.zhihu.com\\/story\\/9719798\",\"image\":\"https:\\/\\/pic3.zhimg.com\\/v2-7fd42dc941768d7b1b1e7c81962eef0a.jpg\",\"title\":\"为什么会有文学鄙视链的存在，网络文学真的难登大雅之堂吗？\",\"ga_prefix\":\"013116\",\"type\":0,\"id\":9719798}]}";
            return json;
        }

        private TextWriter CreateWriter(string fileName)
        {
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestResults");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, fileName + ".log");
            var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            return new StreamWriter(stream);
        }

        private void WriteLine(TextWriter writer, string message)
        {
            message = $"{DateTime.Now}:{message}";
            writer?.WriteLine(message);
            writer?.Flush();
        }
    }
}
