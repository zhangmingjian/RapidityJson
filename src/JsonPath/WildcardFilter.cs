using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.JsonPath
{
    /// <summary>
    /// 通配符。所有对象/元素
    /// </summary>
    public class WildcardFilter : JsonPathFilter
    {
        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            foreach (var element in current)
            {
                switch (element.ElementType)
                {
                    case JsonElementType.Object:
                        var obj = (JsonObject)element;
                        foreach (var name in obj.GetPropertyNames())
                        {
                            yield return obj[name];
                        }
                        break;
                    case JsonElementType.Array:
                        foreach (var item in (JsonArray)element)
                        {
                            yield return item;
                        }
                        break;
                    default: yield return element; break;
                }
            }
        }
    }

    /// <summary>
    /// 递归下降
    /// </summary>
    public class RecursiveFilter : JsonPathFilter
    {
        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            return current;
        }
    }
}
