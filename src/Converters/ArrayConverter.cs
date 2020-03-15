using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rapidity.Json.Converters
{
    //internal class ArrayConverter : EnumerableConverter, IConverterCreator
    //{
    //    private Type _arrayType;

    //    public ArrayConverter(Type type, Type listType, Type itemType) :
    //        base(listType, itemType)
    //    {
    //        _arrayType = type;
    //    }

    //    protected override Func<object, IEnumerator> BuildGetEnumberatorMethod(Type type)
    //    {
    //        return base.BuildGetEnumberatorMethod(_arrayType);
    //    }

    //    private Func<object, object> _toArray;
    //    public Func<object, object> ToArray => _toArray = _toArray ?? BuildToArrayMethod(Type);

    //    protected Func<object, object> BuildToArrayMethod(Type type)
    //    {
    //        var objExp = Expression.Parameter(typeof(object), "list");

    //        var equalExp = Expression.Equal(objExp, Expression.Constant(null));

    //        var listExp = Expression.TypeAs(objExp, type);
    //        var method = type.GetMethod("ToArray");
    //        var call = Expression.Call(listExp, method);
    //        //判断入参是否为null
    //        var body = Expression.Condition(equalExp,
    //                            objExp,
    //                            Expression.TypeAs(call, typeof(object)));
    //        var expression = Expression.Lambda<Func<object, object>>(body, objExp);
    //        return expression.Compile();
    //    }

    //    public override bool CanConvert(Type type)
    //    {
    //        return type.IsArray;
    //    }

    //    public override ITypeConverter Create(Type type)
    //    {
    //        var elementType = type.GetElementType();
    //        var listType = typeof(List<>).MakeGenericType(elementType);
    //        return new ArrayConverter(type, listType, elementType);
    //    }

    //    public override object FromReader(JsonReader reader, JsonOption option)
    //    {
    //        var list = base.FromReader(reader, option);
    //        return ToArray(list);
    //    }

    //    public override object FromToken(JsonToken token, JsonOption option)
    //    {
    //        var list = base.FromToken(token, option);
    //        return ToArray(list);
    //    }
    //}

    /// <summary>
    /// 数组序列化器
    /// </summary>
    internal class ArrayConverter : TypeConverterBase, IConverterCreator
    {
        private const int InitialLength = 2;
        private Type _elementType;

        public ArrayConverter(Type type) : base(type)
        {
            _elementType = type?.GetElementType();
        }

        public bool CanConvert(Type type)
        {
            return type.IsArray;
        }

        public ITypeConverter Create(Type type)
        {
            return new ArrayConverter(type);
        }

        public override object FromReader(JsonReader reader, JsonOption option)
        {
            Array array = null;
            var index = -1;
            var convert = option.ConverterProvider.Build(_elementType);
            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.None: break;
                    case JsonTokenType.EndArray:
                        return ArrayCopy(array, index);
                    case JsonTokenType.StartArray:
                        array = Array.CreateInstance(_elementType, InitialLength);
                        break;
                    case JsonTokenType.Null:
                        if (array == null) return array;
                        SetValue(null, ref array, ref index);
                        break;
                    default:
                        var value = convert.FromReader(reader, option);
                        SetValue(value, ref array, ref index);
                        break;
                }
            } while (reader.Read());
            throw new JsonException($"无效的JSON Token: {reader.TokenType},序列化对象:{Type},应为：{JsonTokenType.StartArray}[", reader.Line, reader.Position);
        }

        private void SetValue(object item, ref Array array, ref int index)
        {
            if (index + 1 >= array.Length)
            {
                var newArray = Array.CreateInstance(_elementType, array.Length == 0 ? InitialLength : array.Length * 2);
                Array.Copy(array, newArray, array.Length);
                array = newArray;
            }
            array.SetValue(item, ++index);
        }

        private Array ArrayCopy(Array array, int index)
        {
            var length = index + 1;
            if (length == array.Length) return array;
            var newArray = Array.CreateInstance(_elementType, length);
            Array.Copy(array, newArray, length);
            return newArray;
        }

        public override object FromToken(JsonToken token, JsonOption option)
        {

            switch (token.ValueType)
            {
                case JsonValueType.Null: return null;
                case JsonValueType.Array:
                    var arrayToken = (JsonArray)token;
                    Array array = Array.CreateInstance(_elementType, InitialLength);
                    var index = -1;
                    var convert = option.ConverterProvider.Build(_elementType);
                    foreach (var item in arrayToken)
                    {
                        var itemValue = convert.FromToken(item, option);
                        SetValue(itemValue, ref array, ref index);
                    }
                    return ArrayCopy(array, index);
                default:
                    throw new JsonException($"无法从{token.ValueType}转换为{Type},{this.GetType().Name}反序列化{Type}失败");
            }
        }

        public override void ToWriter(JsonWriter writer, object obj, JsonOption option)
        {
            var array = (Array)obj;
            writer.WriteStartArray();
            for (int i = 0; i < array.Length; i++)
            {
                var value = array.GetValue(i);
                //循环引用处理
                if (HandleLoopReferenceValue(writer, value, option))
                    continue;
                base.ToWriter(writer, value, option);
            }
            writer.WriteEndArray();
            option.LoopReferenceChecker.PopObject();
        }
    }
}
