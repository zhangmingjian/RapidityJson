using Rapidity.Json.JsonPath;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xunit;

namespace Rapidity.Json.Test
{
    public class JsonPathResolveTest
    {
        [Fact]
        public void ResolveFilterTest()
        {
            //var path = "$.*.rootname['name.data','list']";
            //var path = "$.[city,0,'address',:2:3]";
            //var path = "$.[phoneNumbers,0,'type']";
            var path = "$ ";
            var resolver = new DefaultJsonPathResolver();
            var filters = resolver.ResolveFilters(path).ToList();

            var str = "?(@.age=01)";
            var str1 = str.TrimStart('?','(').TrimEnd(')');
        }


        [Fact]
        public void PathFilterTest1()
        {
            var path = "*.city";
            var json = "{\"firstName\":\"John\",\"lastName\":\"doe\",\"age\":26,\"address\":{\"streetAddress\":\"naist street\",\"city\":\"Nara\",\"postalCode\":\"630 - 0192\"},\"phoneNumbers\":[{\"type\":\"iPhone\",\"number\":\"0123 - 4567 - 8888\"},{\"type\":\"home\",\"number\":\"0123 - 4567 - 8910\"}]}";
            var element = JsonElement.Create(json);

            var filters = new DefaultJsonPathResolver().ResolveFilters(path).ToList();

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

        [Fact]
        public void ElementFiltersTest()
        {
            var json = "{\"firstName\":\"John\",\"lastName\":\"doe\",\"age\":26,\"address\":{\"streetAddress\":\"naist street\",\"city\":\"Nara\",\"postalCode\":\"630 - 0192\"},\"phoneNumbers\":[{\"type\":\"iPhone\",\"number\":\"0000000\"},{\"type\":\"home\",\"number\":\"111111111\"}]}";
            var element = JsonElement.Create(json);
            var filters = new DefaultJsonPathResolver().ResolveFilters("$.[?(@.age>10)]").ToList();

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

            var jtoken = Newtonsoft.Json.Linq.JToken.Parse(json);
            var res = jtoken.SelectTokens("$.[?(@.age>10)]").ToList();
        }
    }
}