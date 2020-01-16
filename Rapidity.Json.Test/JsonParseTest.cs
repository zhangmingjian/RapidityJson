using Rapidity.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Rapidity.Json.Tests
{
    public class JsonParseTest
    {
        [Fact]
        public void ParseObjectTest()
        {
            var json = "{'id':'jwjeflwei金风科技了',\"NAME\":'\u12df\uff4a你好\\//',\"url\":'http:\\/\\/www.baidu.com'}";
            var person = new JsonParser().To<Person>(json);
        }

        class Person
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string Url { get; set; }
        }
    }

}
