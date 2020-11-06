using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Rapidity.Json.JsonPath
{
    /// <summary>
    /// 表达式过滤器  [?(<expression>)] 过滤表达式。 表达式必须求值为一个布尔值。</summary>
    /// </summary>
    internal class ExpressionFilter : JsonPathFilter
    {
        private MatchExpression _expression;

        public ExpressionFilter(string expText) : this(MatchExpression.Create(expText))
        {
        }

        public ExpressionFilter(MatchExpression expresstion)
        {
            _expression = expresstion;
        }

        public override IEnumerable<JsonElement> Filter(JsonElement root, IEnumerable<JsonElement> current)
        {
            foreach (var element in current)
            {
                if (_expression.IsMatch(root, element))
                    yield return element;
            }
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

        public abstract bool IsMatch(JsonElement root, JsonElement current);

        public static MatchExpression Create(string expression)
        {
            //todo
            return null;
        }
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

        public override bool IsMatch(JsonElement root, JsonElement current)
        {
            switch (ConditionType)
            {
                case ConditionType.And: return Left.IsMatch(root, current) && Right.IsMatch(root, current);
                case ConditionType.Or: return Left.IsMatch(root, current) || Right.IsMatch(root, current);
                default: throw new NotSupportedException();
            }
        }
    }

    /// <summary>
    /// 条件匹配 查询表达式
    /// </summary>
    internal class ConditionMatchExpression : MatchExpression
    {
        private ElementSelector _left;
        private ElementSelector _right;

        public ConditionMatchExpression(ConditionType type, ElementSelector left) : this(type, left, null)
        {
        }

        public ConditionMatchExpression(ConditionType type, ElementSelector left, ElementSelector right)
        {
            ConditionType = type;
            _left = left;
            _right = right;
        }

        public override bool IsMatch(JsonElement root, JsonElement current)
        {
            var leftValue = _left.Select(root, current);
            var rightValue = _right.Select(root, current);
            switch (ConditionType)
            {
                case ConditionType.None: //无条件时 current非空即可满足
                    if (leftValue is JsonBoolean jsonBoolean) return jsonBoolean.Value;
                    if (leftValue.ElementType == JsonElementType.Null) return false;
                    if (leftValue is JsonString jsonString) return !string.IsNullOrEmpty(jsonString.Value);
                    return leftValue != null;

                case ConditionType.GreaterThan:
                case ConditionType.LessThan:
                case ConditionType.GreaterOrEqual:
                case ConditionType.LessOrEqual: return NumberCompare(leftValue, rightValue);

                case ConditionType.Equal:
                    {
                        if (leftValue is JsonNumber number1 && rightValue is JsonNumber nuber2) return number1 == nuber2;
                        if (leftValue is JsonString string1 && rightValue is JsonString string2) return string1 == string2;
                        if (leftValue is JsonBoolean boolean1 && rightValue is JsonBoolean boolean2) return boolean1 == boolean2;
                        return false;
                    }
                case ConditionType.NotEqual:
                    {
                        if (leftValue is JsonNumber number1 && rightValue is JsonNumber nuber2) return number1 != nuber2;
                        if (leftValue is JsonString string1 && rightValue is JsonString string2) return string1 != string2;
                        if (leftValue is JsonBoolean boolean1 && rightValue is JsonBoolean boolean2) return boolean1 != boolean2;
                        return false;
                    }
                case ConditionType.Regular:
                    if (!(leftValue is JsonString str)) return false;
                    var regular = ((JsonString)rightValue).Value;
                    return Regex.IsMatch(str.Value, regular);

                default: throw new NotSupportedException();
            }
        }

        private bool NumberCompare(JsonElement left, JsonElement right)
        {
            if (!(left is JsonNumber number1) || !(right is JsonNumber number2))
                return false;
            switch (ConditionType)
            {
                case ConditionType.GreaterThan: return number1 > number2;
                case ConditionType.LessThan: return number1 < number2;
                case ConditionType.GreaterOrEqual: return number1 >= number2;
                case ConditionType.LessOrEqual: return number1 <= number2;
            }
            return false;
        }
    }

    #region json元素属性节点选择器
    internal abstract class ElementSelector
    {
        public abstract JsonElement Select(JsonElement root, JsonElement current);
    }

    internal class ConstantSelector : ElementSelector
    {
        private JsonElement _element;
        public ConstantSelector(JsonElement element)
        {
            _element = element;
        }
        public override JsonElement Select(JsonElement root, JsonElement current)
        {
            return _element;
        }
    }

    internal class CurrentSelector : ElementSelector
    {
        private JsonPathFilter _filter;
        public CurrentSelector(JsonPathFilter filter)
        {
            _filter = filter;
        }
        public override JsonElement Select(JsonElement root, JsonElement current)
        {
            var result = _filter.Filter(root, new JsonElement[] { current });
            if (result == null || !result.Any()) return new JsonNull();
            return result.First();
        }
    }

    internal class RootSelector : ElementSelector
    {
        private JsonPathFilter _filter;
        public RootSelector(JsonPathFilter filter)
        {
            _filter = filter;
        }
        public override JsonElement Select(JsonElement root, JsonElement current)
        {
            var result = _filter.Filter(root, new JsonElement[] { root });
            if (result == null || !result.Any()) return new JsonNull();
            return result.First();
        }
    }
    #endregion
}