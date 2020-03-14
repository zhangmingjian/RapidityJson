using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Rapidity.Json.Test
{
    public class JsonTokenTest
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
    }
}
