using Rapidity.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Rapidity.Json.Test
{
    public class JsonWriterTest
    {
        private ITestOutputHelper _output;
        public JsonWriterTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void StreamWriteTest()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "write.json";
            var file = new FileInfo(path);
            var stream = file.OpenWrite();
            var textWriter = new StreamWriter(stream);
            var writer = new JsonWriter(textWriter, new JsonOption() { SkipValidated = true });
            writer.WriteString("afeawefewf");
            writer.WriteString("afeawefewf");
            var json = writer.ToString();
            writer.Dispose();
        }

        /// <summary>
        /// InvalidStartToken
        /// </summary>
        [Fact]
        public void InvalidStartTokenTest()
        {
            var sw = new StringWriter();
            using (var write = new JsonWriter(sw))
            {
                //EndObject
                Assert.Throws<JsonException>(() => write.WriteEndObject());
                //EndArray
                Assert.Throws<JsonException>(() => write.WriteEndArray());
                //PropertyName
                Assert.Throws<JsonException>(() => write.WritePropertyName("property"));

                //Multiple value token
                write.WriteFloat(-0.1f);
                Assert.Throws<JsonException>(() => write.WriteLong(100));
            }
        }

        [Fact]
        public void InvalidEndTokenTest()
        {
            using (var sw = new StringWriter())
            using (var write = new JsonWriter(sw))
            {
                write.WriteStartArray();
                Assert.Throws<JsonException>(() => write.WriteEndObject());
            }

            using (var sw = new StringWriter())
            using (var write = new JsonWriter(sw))
            {
                write.WriteStartArray();
                Assert.Throws<JsonException>(() => write.WritePropertyName("aaef"));
            }

            using (var sw = new StringWriter())
            using (var write = new JsonWriter(sw))
            {
                write.WriteStartObject();
                Assert.Throws<JsonException>(() => write.WriteString("afwwfwe"));
            }


            using (var sw = new StringWriter())
            using (var write = new JsonWriter(sw))
            {
                write.WriteStartObject();
                Assert.Throws<JsonException>(() => write.WriteChar('\a'));
            }

            using (var sw = new StringWriter())
            using (var write = new JsonWriter(sw))
            {
                write.WriteStartObject();
                Assert.Throws<JsonException>(() => write.WriteInt(1000));
            }

            using (var sw = new StringWriter())
            using (var write = new JsonWriter(sw))
            {
                write.WriteStartObject();
                Assert.Throws<JsonException>(() => write.WriteBoolean(true));
            }

            using (var sw = new StringWriter())
            using (var write = new JsonWriter(sw))
            {
                write.WriteStartObject();
                Assert.Throws<JsonException>(() => write.WriteNull());
            }

            using (var sw = new StringWriter())
            using (var write = new JsonWriter(sw))
            {
                write.WriteStartObject();
                Assert.Throws<JsonException>(() => write.WriteEndArray());
            }

            using (var sw = new StringWriter())
            using (var write = new JsonWriter(sw))
            {
                write.WriteBoolean(false);
                Assert.Throws<JsonException>(() => write.WriteEndArray());
            }
        }

        [Fact]
        public void InvalidJsonTokenTest()
        {
            using (var sw = new StringWriter())
            using (var write = new JsonWriter(sw))
            {
                write.WriteStartObject();
                write.WritePropertyName("afeawe");
                write.WriteStartArray();
                write.WriteString("value");
                write.WriteString("WriteEndObject");
                write.WriteStartObject();
                write.WritePropertyName("name");
                write.WriteNull();
                write.WritePropertyName("remark");
                write.WriteStartObject();
                write.WriteEndObject();

                //write.WriteEndObject(); //缺少一个结束括号
                Assert.Throws<JsonException>(() => write.WriteNull());
            }
        }

        /// <summary>
        /// 缩进测试
        /// </summary>
        [Fact]
        public void WriteIndentedTest()
        {
            var option = new JsonOption
            {
                //SkipValidated = true,
                IndenteLength = 4,
                //UseSingleQuote = true
            };
            using (var sw = new StringWriter())
            using (var write = new JsonWriter(sw, option))
            {
                write.WriteStartObject();

                write.WritePropertyName("property1property1property1property1");
                write.WriteStartArray();
                write.WriteString("WriteEndObjefect");
                write.WriteDateTime(DateTime.Now);
                write.WriteBoolean(true);
                write.WriteNull();
                write.WriteLong(435890);
                write.WriteFloat(0.256845f);
                write.WriteDouble(1254E+02);
                write.WriteStartObject();
                write.WritePropertyName("属性1");
                write.WriteString("stringvalue2");
                write.WritePropertyName("属性2");
                write.WriteStartObject();
                write.WritePropertyName("PropertyName");
                write.WriteStartObject();
                write.WritePropertyName("PropertyName2");
                write.WriteGuid(Guid.NewGuid());
                write.WriteEndObject();
                write.WriteEndObject();
                write.WriteEndObject();
                write.WriteEndArray();

                write.WritePropertyName("peroperty2");
                write.WriteStartObject();
                write.WritePropertyName("属性萨芬");
                write.WriteStartArray();
                write.WriteEndArray();
                write.WriteEndObject();

                write.WriteEndObject();

                _output.WriteLine(sw.ToString());
            }
        }

        /// <summary>
        /// 缩进测试
        /// </summary>
        [Fact]
        public void WriteIndentedTest2()
        {
            var option = new JsonOption
            {
                //SkipValidated = true,
                IndenteLength = 4,
                DateTimeFormat = "yyyyMMddHHmmss"
            };
            using (var sw = new StringWriter())
            using (var write = new JsonWriter(sw, option))
            {
                var watch = Stopwatch.StartNew();
                write.WriteStartArray();

                write.WriteStartArray();
                write.WriteString("WriteEndObjefect");
                write.WriteDateTime(DateTime.Now);
                write.WriteBoolean(true);
                write.WriteString("\u5927\u732a\u8e44\u5b50");
                write.WriteNull();
                write.WriteLong(435890);
                write.WriteFloat(0.256845f);
                write.WriteDouble(1254E+02);
                write.WriteStartObject();
                write.WritePropertyName("属性1");
                write.WriteString("飞机卡拉  发我u覅我");
                write.WritePropertyName("属性2");
                write.WriteStartObject();
                write.WritePropertyName("PropertyName");
                write.WriteStartObject();
                write.WritePropertyName("PropertyName2");
                write.WriteGuid(Guid.NewGuid());
                write.WritePropertyName("第二个属性");
                write.WriteStartObject();
                write.WritePropertyName("diergeshuxingchildnode");
                write.WriteString((string)null);
                write.WriteEndObject();
                write.WriteEndObject();
                write.WriteEndObject();
                write.WriteEndObject();
                write.WriteEndArray();

                write.WriteStartObject();
                write.WritePropertyName("属性萨芬");
                write.WriteStartArray();
                write.WriteEndArray();
                write.WriteEndObject();

                write.WriteEndArray();

                watch.Stop();
                _output.WriteLine(sw.ToString());
                _output.WriteLine($"用时：{watch.ElapsedMilliseconds}ms");
            }
        }

        [Fact]
        public void WriteRawTest()
        {
            var option = new JsonOption
            {
                //Indented = true
            };
            using (var sw = new StringWriter())
            using (var write = new JsonWriter(sw, option))
            {
                write.WriteStartObject();
                write.WritePropertyName("userName");
                write.WriteRaw("{\"_object\" : { 'property' : \" 特色tea为了开发了房间卡拉文件 \"}}");
                write.WriteEndObject();
                var json = sw.ToString();
            }
        }

        [Fact]
        public void WriteRawJsonTest()
        {
            var option = new JsonOption
            {
                Indented = true,
                //UseSingleQuote = true
            };
            using (var sw = new StringWriter())
            using (var write = new JsonWriter(sw, option))
            {
                write.WriteStartObject();
                write.WritePropertyName("userName");
                write.WriteString("{\"_object\" : { \"property\" : \" 特色\a\b\r\n\f\t\v\u5653\u0007\u000b为了开发了房间/卡拉\\\\文件 \"}}");
                write.WritePropertyName("url");
                write.WriteString("http:\\/\\/wwww.baidu.com");
                write.WriteEndObject();
                var json = sw.ToString();
                _output.WriteLine(json);

                var token = (JsonObject)JsonToken.Parse(json);
                var token2 = (JsonObject)JsonToken.Parse(token.ToString());
            }
        }

        private string GetJson()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "large.json");
            return File.ReadAllText(path, Encoding.UTF8);
        }

        /// <summary>
        /// 先读后写
        /// </summary>
        [Fact]
        public void ReadToWriteTest()
        {
            var json = GetJson();

            var token = JsonToken.Parse(json);
            _output.WriteLine(token.ToString());
        }

        /// <summary>
        /// 先读后写入文件
        /// </summary>
        [Fact]
        public void ReadToWriteFileTest()
        {
            var json = GetJson();

            var token = JsonToken.Parse(json);

            var file = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "write.json");
            var stream = file.OpenWrite();
            var textWriter = new StreamWriter(stream);
            var watch = Stopwatch.StartNew();

            var option = new JsonOption
            {
                SkipValidated = true,
                IndenteLength = 2,
            };
            using (var write = new JsonWriter(textWriter, option))
            {
                write.WriteToken(token);
                watch.Stop();
                _output.WriteLine($"用时：{watch.ElapsedMilliseconds}ms");
            }
        }

        [Fact]
        public void NewtonsoftJsonWriteTest()
        {
            var stringWriter = new StringWriter();
            using (var write = new Newtonsoft.Json.JsonTextWriter(stringWriter))
            {
                write.IndentChar = '\t';
                write.Indentation = 1;
                write.Formatting = Newtonsoft.Json.Formatting.Indented;
                write.WriteStartObject();
                write.WritePropertyName("userName");
                write.WriteValue("{\"_object\" : { \"property\" : \" 特色\a\b\r\n\f\t\v\u5650\u231a为了开发了房间/卡拉\\\\文件 \"}}");
                write.WritePropertyName("url");
                write.WriteValue("http:\\/\\/wwww.baidu.com");
                write.WriteEndObject();
                var json = stringWriter.ToString();
                _output.WriteLine(json);
                var token = Newtonsoft.Json.Linq.JToken.Parse(json);
                var token2 = Newtonsoft.Json.Linq.JToken.Parse(token.ToString());
            }
        }

        [Fact]
        public void EncodingTest()
        {
            //var bytes = Encoding.UTF8.GetBytes("你");
            //var str = Encoding.UTF8.GetChars(bytes);
            var base64 = "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";
            var array1 = Convert.FromBase64String(base64);

            var len = base64.Length > 0 && base64[base64.Length - 1] == '='
                ? base64.Length > 1 && base64[base64.Length - 2] == '=' ? 2 : 1
                : 0;
            byte[] array2 = new byte[((base64.Length * 3) + 3) / 4 - len];
            bool success = Convert.TryFromBase64String(base64, array2, out int written);
            //var b = Convert.TryFromBase64String(base64, array2, out int count);

            var base1 = Convert.ToBase64String(array1);
            var base2 = Convert.ToBase64String(array2);

            Assert.Equal(base1, base64);
            Assert.Equal(base2, base64);
            var time = DateTime.Now.ToString(CultureInfo.CurrentCulture);
            var time2 = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }
    }
}