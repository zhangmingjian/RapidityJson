using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.JsonPath
{
    public class PathNameFilter : JsonPathFilter
    {
        private string _name;
        public PathNameFilter(string name)
        {
            this._name = name;
        }

        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            foreach(var element in current)
            {
                switch (element.ElementType)
                {
                    case JsonElementType.Object:
                        var obj = (JsonObject)element;
                        if (obj.ContainsProperty(_name)) yield return obj[_name];
                        break;
                }
            }
        }
    }
}
