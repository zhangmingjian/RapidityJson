using Rapidity.Json;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Rapidity.Json
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
                var value = read.Value?.ToString() ?? string.Empty;
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
                var token = new JsonParser().Parse(json);
                watch.Stop();
                _output.WriteLine($"用时：{watch.ElapsedMilliseconds}ms");
                var str = token.ToString();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void ReadPerformanceTest()
        {
            var json = GetJson();
            //jsonreader
            {
                var watch = Stopwatch.StartNew();
                int count = 0;
                using (var read = new JsonReader(json))
                {
                    while (read.Read())
                    {
                        count++;
                    }
                }
                watch.Stop();
                _output.WriteLine($"json读用时：{watch.ElapsedMilliseconds}ms,token数量：{count}");
            }

            {
                var watch = Stopwatch.StartNew();
                var token = new JsonParser().Parse(json);
                watch.Stop();
                _output.WriteLine($"json读+解析用时：{watch.ElapsedMilliseconds}ms");
            }

            //newtonsoft.jsonreader
            {
                var watch = Stopwatch.StartNew();
                int count = 0;
                using (var read = new Newtonsoft.Json.JsonTextReader(new StringReader(json)))
                {
                    while (read.Read())
                    {
                        count++;
                    }
                }
                watch.Stop();
                _output.WriteLine($"newtonsoft读用时：{watch.ElapsedMilliseconds}ms,token数量：{count}");


            }

            {
                var watch = Stopwatch.StartNew();
                var token = Newtonsoft.Json.Linq.JToken.Parse(json);
                watch.Stop();
                _output.WriteLine($"newtonsoft读+解析用时：{watch.ElapsedMilliseconds}ms");
            }

            //microsoft Json
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                var memory = new ReadOnlySpan<byte>(bytes);
                var read = new System.Text.Json.Utf8JsonReader(memory);
                var watch = Stopwatch.StartNew();
                int count = 0;
                while (read.Read())
                {
                    count++;
                }
                watch.Stop();
                _output.WriteLine($"system.json读用时：{watch.ElapsedMilliseconds}ms,token数量：{count}");

            }
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                var watch = Stopwatch.StartNew();
                var doc = System.Text.Json.JsonDocument.Parse(new ReadOnlyMemory<byte>(bytes));
                watch.Stop();
                _output.WriteLine($"system.json读+解析用时：{watch.ElapsedMilliseconds}ms");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void ReadQuoteTest()
        {
            var json = "{'name':'张三\','age':10,'\"remark\"':'安慰剂flaw金额flak文件'}";
            var data = new JsonParser().Parse(json);
        }

        [Fact]
        public void ReadEscapeStringTest()
        {
            var json = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample.json"));
            var token = JsonToken.Parse(json);
            var j = token.ToString();
        }
    }
}