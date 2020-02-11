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
            var json = "[]{}";
            var read = new JsonReader(json);
            Assert.Throws<JsonException>(() =>
            {
                while (read.Read())
                {
                    _output.WriteLine(read.TokenType.ToString());
                }
            });
        }

        /// <summary>
        /// JsonReader Parser 性能测试
        /// </summary>
        [Fact(DisplayName = "JsonReader性能测试")]
        public void ReadPerformanceTest()
        {
            var json = GetJson();
            int total = 5;
            //jsonreader
            {
                _output.WriteLine("==Rapidity.Json================");
                var watch = Stopwatch.StartNew();
                for (int i = 1; i <= total; i++)
                {
                    int count = 0;
                    using (var read = new JsonReader(json))
                    {
                        while (read.Read())
                        {
                            count++;
                        }
                    }
                }
                watch.Stop();
                _output.WriteLine($"Rapidity.Json读{total}次用时：{watch.ElapsedMilliseconds}ms");
            }

            {
                var watch = Stopwatch.StartNew();
                for (int i = 1; i <= total; i++)
                {
                    var token = JsonToken.Parse(json);
                }
                watch.Stop();
                _output.WriteLine($"Rapidity.Json读+解析{total}次用时：{watch.ElapsedMilliseconds}ms");
            }

            //newtonsoft.jsonreader
            {
                _output.WriteLine("==Newtonsoft.Json================");
                var watch = Stopwatch.StartNew();
                for (int i = 1; i <= total; i++)
                {
                    int count = 0;
                    using (var read = new Newtonsoft.Json.JsonTextReader(new StringReader(json)))
                    {
                        while (read.Read())
                        {
                            count++;
                        }
                    }
                }
                watch.Stop();
                _output.WriteLine($"Newtonsoft.Json读{total}次用时：{watch.ElapsedMilliseconds}ms");
            }

            {
                var watch = Stopwatch.StartNew();
                for (int i = 1; i <= total; i++)
                {
                    var token = Newtonsoft.Json.Linq.JToken.Parse(json);
                }
                watch.Stop();
                _output.WriteLine($"Newtonsoft.Json读+解析{total}次用时：{watch.ElapsedMilliseconds}ms");

            }

            //microsoft Json
            {
                _output.WriteLine("==System.Text.Json================");
                var bytes = Encoding.UTF8.GetBytes(json);
                var memory = new ReadOnlySpan<byte>(bytes);
                var watch = Stopwatch.StartNew();
                for (int i = 1; i <= total; i++)
                {
                    var read = new System.Text.Json.Utf8JsonReader(memory);
                    int count = 0;
                    while (read.Read())
                    {
                        count++;
                    }
                }
                watch.Stop();
                _output.WriteLine($"System.Text.Json读{total}次用时：{watch.ElapsedMilliseconds}ms");
            }
            {
                var bytes = Encoding.UTF8.GetBytes(json);
                var watch = Stopwatch.StartNew();
                for (int i = 1; i <= total; i++)
                {
                    var doc = System.Text.Json.JsonDocument.Parse(new ReadOnlyMemory<byte>(bytes));
                }
                watch.Stop();
                _output.WriteLine($"System.Text.Json读+解析{total}次用时：{watch.ElapsedMilliseconds}ms");
            }
        }

        [Fact]
        public void ReadStructureTest()
        {
            int total = 20000;
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