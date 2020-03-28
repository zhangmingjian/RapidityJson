using Rapidity.Json;
using Rapidity.Json.Test;
using Rapidity.Json.Test.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Rapidity.Json.Test
{
    public class JsonParseTest
    {
        private ITestOutputHelper _output;

        public JsonParseTest(ITestOutputHelper helper)
        {
            _output = helper;
        }

        [Fact]
        public void ParseObjectTest()
        {
            var json = "{\"id\":100,\"Name\":\"张三\",\"Sex\":1,\"Birthday\":\"2000-10-10\"}";
            var student = JsonParse.To<Student>(json);
        }

        [Fact]
        public void ObjectToJsonTest()
        {
            var student = new Student
            {
                Id = 111,
                Name = "testName",
                Sex = Sex.Unkown,
                Address = "北京市海淀区",
                //Birthday = DateTime.Now
            };
            var json = JsonParse.ToJson(student);
            //{"Id":111,"Name":"testName","Sex":"Unkown","Birthday":"2020-02-15 17:43:31","Address":"北京市海淀区"}
            var option = new JsonOption
            {
                WriteEnumValue = true, //序列化时使用枚举值
                DateTimeFormat = "yyyy-MM-dd" //指定datetime格式
            };
            var json2 = JsonParse.ToJson(student, option);
            //{"Id":111,"Name":"testName","Sex":0,"Birthday":"2020-02-15","Address":"北京市海淀区"}
        }

        [Fact]
        public void ParseListTest()
        {
            var json = "[{\"id\":100,\"Name\":\"张三\",\"Sex\":1,\"Birthday\":\"2000 - 10 - 10\"},{\"id\":101,\"Name\":\"李四\",\"Sex\":\"female\",\"Birthday\":null,\"Address\":\"\"}]";
            var list = JsonParse.To<List<Student>>(json);
            var list2 = JsonParse.To<IEnumerable<Student>>(json);
            var list3 = JsonParse.To<IList<Student>>(json);
            var arr = JsonParse.To<Student[]>(json);
        }

        [Fact]
        public void ListToJsonTest()
        {
            var list = new List<Student>
            {
                new Student {Id=123,Name="username1",Sex=Sex.Male,Birthday = new DateTime(1980,1,1) },
                new Student {Id=125,Name="username2",Sex=Sex.Female},
            };
            var json1 = JsonParse.ToJson(list, true); //使用缩进格式
            /*
            [
	            {
		            "Id":123,
		            "Name":"username1",
		            "Sex":"Male",
		            "Birthday":"1980-01-01 00:00:00",
		            "Address":null
	            },
	            {
		            "Id":125,
		            "Name":"username2",
		            "Sex":"Female",
		            "Birthday":null,
		            "Address":null
	            }
            ] 
            */
            var option = new JsonOption
            {
                Indented = true,    //缩进格式
                DateTimeFormat = "yyyy-MM-dd",
                IgnoreNullValue = true //忽略null输出
            };
            var json2 = JsonParse.ToJson(list, option);
            /*
               [
	                {
		                "Id":123,
		                "Name":"username1",
		                "Sex":"Male",
		                "Birthday":"1980-01-01"
	                },
	                {
		                "Id":125,
		                "Name":"username2",
		                "Sex":"Female"
	                }
                ]
             */
        }

        [Fact]
        public void ListObjectToJsonTest()
        {
            var list = new List<object>();
            list.Add("1111");
            list.Add(null);
            var jObj = new JsonObject();
            jObj["aaaa"] = 1111;
            jObj["bbbb"] = "bbbb";
            list.Add(jObj);

            var json = JsonParse.ToJson(list);
        }

        [Fact]
        public void ParseDictionaryTest()
        {
            var json = "{\"确诊病例\":66580,\"疑似病例\":8969,\"治愈病例\":8286,\"死亡病例\":1524}";
            var dic = JsonParse.To<Dictionary<string, int>>(json);
            Assert.Equal(4, dic.Count);
            Assert.Equal(66580, dic["确诊病例"]);

            var dic2 = JsonParse.To<IDictionary<string, int>>(json);
            Assert.Equal(4, dic2.Count);
            Assert.Equal(66580, dic2["确诊病例"]);
        }

        [Fact]
        public void ObjectClassToJsonTest()
        {
            var obj = new MultipleTypesModel();
            obj.Single = new ValueModel
            {
                StringValue = "因为每个人的观赏口味和喜爱风格不尽相同，我会简短介绍每部电影的大致剧情，尽可能把每部电影的特色亮点告诉大家，方便您能选择适合自己的电影观看I strongly suspect that GetType() will take significantly less time than any actual logging. Of course, there's the possibility that your call to Logger.Log won't do any actual IO... I still suspect the difference will be irrelevant though.",
                CharValue = char.MaxValue,
                IntValue = 555555,
                NullIntValue = -121212,
                ByteValue = 255,
                ShortValue = 123,
                LongValue = 6666666666,
                FloatValue = float.NaN,
                DoubleValue = 212.4545456768955667788,
                NullDoubleValue = double.PositiveInfinity,
                DecimalValue = 21212126232.12456899m,
                BoolValue = true,
                NullBoolValue = false,
                DateTimeValue = DateTime.Now,
                NullDateTimeOffsetValue = DateTimeOffset.UtcNow,
                NullGuidValue = Guid.NewGuid(),
                NullDBNullValue = DBNull.Value
            };
            obj.List = new Collection<ValueModel>() { obj.Single };
            obj.Array = new ValueModel[2];
            obj.StructModel = new StructModel
            {
                StringValue = "值类型struct Value",
                ClassModel = obj.Single
            };
            obj.Dictionary = new Dictionary<string, ValueModel>()
            {
                ["key1"] = obj.Single,
                ["key2"] = null
            };
            obj.KeyValuePairs = new List<KeyValuePair<int, ValueModel>>() {
             new KeyValuePair<int, ValueModel>(1,new ValueModel())
            };
            var option = new JsonOption
            {
                LoopReferenceProcess = Converters.LoopReferenceProcess.Error,
                Indented = true
            };
            var json = JsonParse.ToJson(obj, option);
            var token = JsonToken.Parse(json);
            _output.WriteLine(token.ToString());
            var deModel = token.To<MultipleTypesModel>();
        }

        [Fact]
        public void StructToJsonTest()
        {
            var model = new StructModel
            {
                StringValue = Guid.NewGuid().ToString(),
                ClassModel = new ValueModel()
            };
            model.ClassModel.StructModel = new StructModel
            {
                StringValue = Guid.NewGuid().ToString(),
                ClassModel = new ValueModel() { StringValue = DateTime.Now.ToString() }
            };
            var json = JsonParse.ToJson(model, new JsonOption
            {
                IgnoreNullValue = true,
                LoopReferenceProcess = Converters.LoopReferenceProcess.Error
            });
        }

        [Fact]
        public void KeyValuePairsToJsonTest()
        {
            var pairs = new List<KeyValuePair<int, ValueModel>>()
            {
              new KeyValuePair<int, ValueModel>(1, new ValueModel(){ CharValue = char.MaxValue}),
              new KeyValuePair<int, ValueModel>(3, new ValueModel()),
            };
            var option = new JsonOption
            {
                LoopReferenceProcess = Converters.LoopReferenceProcess.Error,
                Indented = true,
                CamelCaseNamed =true
            };
            var json = JsonParse.ToJson(pairs, option);
            _output.WriteLine(json);

            var model = JsonParse.To<List<KeyValuePair<int, ValueModel>>>(json);
        }
    }
}
