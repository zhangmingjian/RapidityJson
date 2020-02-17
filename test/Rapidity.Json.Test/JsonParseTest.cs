using Rapidity.Json;
using Rapidity.Json.Test;
using Rapidity.Json.Test.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Rapidity.Json.Test
{
    public class JsonParseTest
    {
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
                Birthday = DateTime.Now
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
            var obj = new object();
            var json = JsonParse.ToJson(obj);
            //Assert.Equal(obj.ToString(), json);
            
        }

    }
}
