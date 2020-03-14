using Rapidity.Json;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Rapidity.Json.Test
{
    public class JsonReaderTest
    {
        private ITestOutputHelper _output;

        public JsonReaderTest(ITestOutputHelper output)
        {
            _output = output;
        }

        private string GetJson()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "large.json");
            return File.ReadAllText(path, Encoding.UTF8);
        }

        [Fact]
        public void ReadTest()
        {
            Stopwatch watch = Stopwatch.StartNew();
            var read = new JsonReader(GetJson());
            while (read.Read())
            {
                var value = read.Text ?? string.Empty;
                _output.WriteLine(read.TokenType.ToString() + ": " + value);
            }
            watch.Stop();
            _output.WriteLine($"用时：{watch.ElapsedMilliseconds}ms");
        }

        [Fact]
        public void JsonParseTest()
        {
            var json = GetJson();
            {
                var watch = Stopwatch.StartNew();
                var token = Newtonsoft.Json.Linq.JToken.Parse(json);
                watch.Stop();
                _output.WriteLine($"用时：{watch.ElapsedMilliseconds}ms");
                var str = token.ToString();
            }
            {
                var watch = Stopwatch.StartNew();
                var token = JsonToken.Parse(json);
                watch.Stop();
                _output.WriteLine($"用时：{watch.ElapsedMilliseconds}ms");
                var str = token.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void ReadQuoteTest()
        {
            var json = "{'name':'张三\','age':10,'\"remark\"':'安慰剂flaw金额flak文件'}";
            var data = JsonToken.Parse(json);
        }


        [Fact]
        public void ReadEscapeStringTest()
        {
            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample.json"));
            var token = JsonToken.Parse(json);
            var j = token.ToString();
        }

        [Fact]
        public void ReadInvalidTokenTest()
        {
            var json = "[122, ,true]";
            var read = new JsonReader(json);
            Assert.Throws<JsonException>(() =>
            {
                while (read.Read())
                {
                    _output.WriteLine(read.TokenType.ToString());
                }
            });
        }

        [Fact(DisplayName = "JsonReader构造性能测试")]
        public void ReadStructureTest()
        {
            int total = 1_000_000;
            //jsonreader
            {
                _output.WriteLine("==Rapidity.Json================");
                var watch = Stopwatch.StartNew();
                for (int i = 1; i <= total; i++)
                {
                    using (var read = new JsonReader(""))
                    {
                    }
                }
                watch.Stop();
                _output.WriteLine($"Rapidity.JsonReader构造{total}次用时：{watch.ElapsedMilliseconds}ms");
            }
            //newtonsoft.jsonreader
            {
                _output.WriteLine("==Newtonsoft.Json================");
                var watch = Stopwatch.StartNew();
                for (int i = 1; i <= total; i++)
                {
                    using (var read = new Newtonsoft.Json.JsonTextReader(new StringReader("")))
                    {
                    }
                }
                watch.Stop();
                _output.WriteLine($"Newtonsoft.JsonReader构造{total}次用时：{watch.ElapsedMilliseconds}ms");
            }
        }
    }
}