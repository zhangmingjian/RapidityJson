using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.JsonPath
{
    internal class MatchExpressionBuilder
    {
        public static MatchExpression Build(string expression)
        {
            //?(@.age>10) 
            //var cursor = 0;
            //do
            //{
            //    var current = expression[cursor];
            //    switch (current)
            //    {

            //    }
            //    cursor++;
            //}
            //while (cursor < expression.Length);
            //todo
            return new ConditionMatchExpression(ConditionType.Equal, new CurrentSelector(new PropertyFilter("age")), new ConstantSelector(11));
        }
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
