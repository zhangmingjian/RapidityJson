using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rapidity.Json.JsonPath
{
    /// <summary>
    /// 表达式过滤器  [?(<expression>)] 过滤表达式。 表达式必须求值为一个布尔值。</summary>
    /// </summary>
    public class ExpressionFilter : JsonPathFilter
    {
        private string _expression;

        public ExpressionFilter(string expresstion)
        {
            _expression = expresstion;
        }

        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            return current;
        }
    }
}
