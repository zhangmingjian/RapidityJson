using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.JsonPath
{
    public abstract class JsonPathFilter
    {
        public abstract IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current);
    }

    /// <summary>
    /// $ 开头的path
    /// </summary>
    public class RootPathFilter : JsonPathFilter
    {
        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            return new List<JsonElement> { root };
        }
    }

    /// <summary>
    /// 无效jsonpath
    /// </summary>
    public class InvalidPathFilter : JsonPathFilter
    {

        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            return null;
        }
    }
}