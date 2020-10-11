using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rapidity.Json.JsonPath
{
    /// <summary>
    /// 表达式过滤器  [?(<expression>)] 过滤表达式。 表达式必须求值为一个布尔值。</summary>
    /// </summary>
    internal class ExpressionFilter : JsonPathFilter
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

    /// <summary>
    /// 条件查询类型枚举
    /// </summary>
    internal enum ConditionType
    {
        None,           //no condition
        GreaterThan,    //>
        LessThan,       //<   
        GreaterOrEqual, //>=
        LessOrEqual,    //<=
        Equal,          //==
        NotEqual,       //!=
        And,            //&&
        Or,             //||
        Regular,        //Regular Expression
    }

    /// <summary>
    /// 条件符号
    /// </summary>
    internal static class ConditionSymbol
    {
        public const string GreaterThan = ">";
        public const string LessThan = "<";
        public const string GreaterOrEqual = ">=";
        public const string LessOrEqual = "<=";
        public const string Equal = "==";
        public const string NotEqual = "!=";
        public const string And = "&&";
        public const string Or = "||";
        public const string Regular = "";
    }
}