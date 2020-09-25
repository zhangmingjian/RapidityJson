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
            foreach (var element in current)
            {
                switch (element.ElementType)
                {
                    case JsonElementType.Object:
                        var obj = (JsonObject)element;
                        if (obj.ContainsProperty(_name)) yield return obj[_name];
                        else
                        {
                            _name = _name.Trim('\''); //去掉外层的单引号后再次查找一遍
                            if (obj.ContainsProperty(_name)) yield return obj[_name];
                        }
                        break;
                }
            }
        }
    }
}
