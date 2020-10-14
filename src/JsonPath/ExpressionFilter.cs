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
        /// <summary>
        /// 大于 >
        /// </summary>
        GreaterThan,    //>
        /// <summary>
        /// 小于 <
        /// </summary>
        LessThan,       //<   
        /// <summary>
        /// 大于等于 >=
        /// </summary>
        GreaterOrEqual, //>=
        /// <summary>
        /// 小于等于 <=
        /// </summary>
        LessOrEqual,    //<=
        /// <summary>
        /// 等于 ==
        /// </summary>
        Equal,          //==
        /// <summary>
        /// 不等于 !=
        /// </summary>
        NotEqual,       //!=
        Regular,        //Regular Expression
        And,            //&&
        Or,             //||
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

    internal abstract class MatchExpression
    {
        protected ConditionType ConditionType { get; set; }

        public abstract bool CanMatch(JsonElement current);
    }

    /// <summary>
    /// and  or 条件组合表达式
    /// </summary>
    internal class CombineExpression : MatchExpression
    {
        public MatchExpression Left { get; set; }

        public MatchExpression Right { get; set; }

        public CombineExpression(ConditionType type)
        {
            ConditionType = type;
        }

        public CombineExpression(ConditionType type, MatchExpression left, MatchExpression right)
        {
            ConditionType = type;
            Left = left;
            Right = right;
        }

        public override bool CanMatch(JsonElement current)
        {
            switch (ConditionType)
            {
                case ConditionType.And: return Left.CanMatch(current) && Right.CanMatch(current);
                case ConditionType.Or: return Left.CanMatch(current) || Right.CanMatch(current);
                default: throw new NotSupportedException();
            }
        }
    }

    /// <summary>
    /// 条件匹配 查询表达式
    /// </summary>
    internal class ConditionMatchExpression : MatchExpression
    {
        private string _value;

        public ConditionMatchExpression(ConditionType type) : this(type, null)
        {
        }

        public ConditionMatchExpression(ConditionType type, string value)
        {
            ConditionType = type;
            _value = value;
        }

        public override bool CanMatch(JsonElement current)
        {
            switch (ConditionType)
            {
                case ConditionType.None: //无条件时 current非空即可满足
                    if (current is JsonBoolean jsonBoolean) return jsonBoolean.Value;
                    if (current.ElementType == JsonElementType.Null) return false;
                    if (current is JsonString jsonString) return !string.IsNullOrEmpty(jsonString.Value);
                    return current != null;

                case ConditionType.GreaterThan:
                case ConditionType.LessThan:
                case ConditionType.GreaterOrEqual:
                case ConditionType.LessOrEqual: return NumberCompare(current);

                case ConditionType.Equal:
                    if (current is JsonNumber number)
                    {
                        var doubleV = _value.TryToDouble();
                        if (doubleV == null) return false;
                        return number == new JsonNumber(doubleV.Value);
                    }
                    if (current is JsonString jString) return jString == _value;
                    if (current is JsonBoolean jBool)
                    {
                        var boolV = _value.TryToBool();
                        if (boolV == null) return false;
                        return jBool == boolV.Value;
                    }
                    return false;
                case ConditionType.NotEqual: break;

                case ConditionType.Regular: break;

                default: throw new NotSupportedException();
            }
            return false;
        }

        private bool NumberCompare(JsonElement element)
        {
            if (element == null) return false;
            if (!(element is JsonNumber jsonNumber)) return false;

            var doubleV = _value.TryToDouble();
            if (doubleV == null) return false;
            var number2 = new JsonNumber(doubleV.Value);
            switch (ConditionType)
            {
                case ConditionType.GreaterThan: return jsonNumber > number2;
                case ConditionType.LessThan: return jsonNumber < number2;
                case ConditionType.GreaterOrEqual: return jsonNumber >= number2;
                case ConditionType.LessOrEqual: return jsonNumber <= number2;
            }
            return false;
        }
    }
}