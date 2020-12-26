using Rapidity.Json.JsonPath;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Rapidity.Json.Test
{
    public class ExpressionFilterTest
    {
        [Fact]
        public void ExpFilterTest()
        {
            var json = "{\"firstName\":\"John\",\"lastName\":\"doe\",\"age\":10,\"address\":{\"streetAddress\":\"naist street\",\"city\":\"Nara\",\"postalCode\":\"630 - 0192\"},\"phoneNumbers\":[{\"type\":\"iPhone\",\"number\":\"0000000\"},{\"type\":\"home\",\"number\":\"111111111\"}]}";
            var element = JsonElement.Create(json);

            var path = "$[?(@.age=11)]";
            var filters = new DefaultJsonPathResolver().Resolve(path).ToList();

            IEnumerable<JsonElement> current = new List<JsonElement> { element };
            foreach (var filter in filters)
            {
                current = filter.Filter(element, current);
                if (current == null || current.Count() == 0) break;
            }
            if (current == null)
            {
                Debug.WriteLine("未匹配到数据");
                return;
            }
            var arr = new JsonArray(current);
            Debug.WriteLine(arr.ToString(new JsonOption { Indented = true }));
        }   
    }
}
