using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Rapidity.Json.Test
{
    public class JsonTokenTest
    {
        [Fact]
        public void ToObjectTest()
        {
            var obj = new JsonObject();
            obj.AddProperty("id", 100);
            obj.AddProperty("name", "zhangsan");
            obj["Birthday"] = "2000-10";

            var person = obj.To<Person>();
        }
    }
}
