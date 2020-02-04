using Rapidity.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static Rapidity.Json.Tests.ExpressionTest;

namespace Rapidity.Json.Test
{
    public class JsonSerializerTest
    {
        [Fact]
        public void SerializeTest()
        {
            var json = "{\"id\":12345,  \"Name\":\"jfkalefjlaj\",\"birthday\":\"2020-02-02 20:10:10\",\"Number\":\"8B961718-F942-4A1C-97EE-50E7F4C0A30C\",\"child\":{\"name\":\"孩子\"}}";
            var reader = new JsonReader(json);
            var serializer = new JsonSerializer();
            var person = serializer.Deserialize<Person>(reader);
        }
    }
}
