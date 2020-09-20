using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Rapidity.Json.Test
{
    public class JsonElementTest
    {
        [Fact]
        public void ToStringTest()
        {
            var obj = new JsonObject();
            obj.AddProperty("id", 100);
            obj.AddProperty("name", "zhangsan");
            obj["Birthday"] = "2000-10";
            obj.AddProperty("loopObject", new JsonNumber(1000));
            var json = obj.ToString(new JsonOption { LoopReferenceProcess = Converters.LoopReferenceProcess.Error });

            var person = obj.To<Person>();
        }

        [Fact]
        public void JsonNumberTest()
        {
            var obj = new JsonObject();
            obj.AddProperty("id", new JsonNumber("12.23324"));
            var number = new JsonNumber("12.23324");
            var json = obj.ToString();
        }

        [Fact]
        public void SelectTest()
        {
            var arr = new JsonArray();

            arr.Add(10);
            arr.Add("fefe");
            arr.Add(false);
            arr.Add(new JsonObject
            {
                ["name"] = "fawejflkwjkef",
                ["other"] = null
            });
            var arr2 = arr.GetArray(0, 10);
            var token = Newtonsoft.Json.Linq.JToken.Parse(arr2.ToString());
            var token2 =   token.SelectTokens("$..name");
        }
    }
}
