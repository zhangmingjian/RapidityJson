using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.JsonPath
{
    /// <summary>
    /// 
    /// </summary>
    public class PathQueryFilter : JsonPathFilter
    {
        private string _query;
        public PathQueryFilter(string query)
        {
            _query = query;
        }

        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            return current;
        }
    }
}