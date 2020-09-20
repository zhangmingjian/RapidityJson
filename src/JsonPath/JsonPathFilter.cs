using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.JsonPath
{
    public abstract class JsonPathFilter
    {
        public abstract IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current);
    }
}