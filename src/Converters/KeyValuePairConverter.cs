using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rapidity.Json.Converters
{
    internal class KeyValuePairConverter : ObjectConverter, IConverterCreator
    {
        //public Type Type { get; }

        public Type KeyType { get; }

        public Type ValueType { get; }

        private const string KeyName = "Key";
        private const string ValueName = "Value";

        public KeyValuePairConverter(Type type, Type keyType, Type valueType) : base(type)
        {
            this.KeyType = keyType;
            this.ValueType = valueType;
        }

        public override bool CanConvert(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
        }

        public override ITypeConverter Create(Type type)
        {
            var args = type.GetGenericArguments();
            return new KeyValuePairConverter(type, args[0], args[1]);
        }

        private Func<object, object, object> _createFunc;
        private Func<object, object, object> CreateFunc
        {
            get
            {
                if (_createFunc == null)
                {
                    var keyParam = Expression.Parameter(typeof(object));
                    var valueParam = Expression.Parameter(typeof(object));
                    var keyExp = Expression.Convert(keyParam, KeyType);
                    var valueExp = Expression.Convert(valueParam, ValueType);
                    var constr = Type.GetConstructor(new Type[] { KeyType, ValueType });
                    var newExp = Expression.New(constr, keyExp, valueExp);
                    var body = Expression.TypeAs(newExp, typeof(object));
                    var lambda = Expression.Lambda<Func<object, object, object>>(body, keyParam, valueParam);
                    _createFunc = lambda.Compile();
                }
                return _createFunc;
            }
        }

        public override object FromReader(JsonReader reader, JsonOption option)
        {
            object key = null;
            object value = null;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.None: break;
                    case JsonTokenType.PropertyName:
                        var property = reader.Text;
                        reader.Read();
                        if (property.Equals(KeyName, StringComparison.OrdinalIgnoreCase))
                        {
                            var convert = option.ConverterProvider.Build(KeyType);
                            key = convert.FromReader(reader, option);
                        }
                        else if (property.Equals(ValueName, StringComparison.OrdinalIgnoreCase))
                        {
                            var convert = option.ConverterProvider.Build(ValueType);
                            value = convert.FromReader(reader, option);
                        }
                        break;
                    case JsonTokenType.EndObject:
                        return CreateFunc(key, value);
                }
            } while (reader.Read());
            throw new JsonException($"无效的JSON Token: {reader.TokenType},序列化对象:{Type},应为:{JsonTokenType.StartObject} {{", reader.Line, reader.Position);
        }

        public override object FromToken(JsonToken token, JsonOption option)
        {
            switch (token.ValueType)
            {
                case JsonValueType.Object:
                    var objToken = (JsonObject)token;
                    object key = null;
                    object value = null;
                    if (objToken.TryGetValue(KeyName, out JsonToken keyToken))
                    {
                        var convert = option.ConverterProvider.Build(KeyType);
                        key = convert.FromToken(keyToken, option);
                    }
                    if (objToken.TryGetValue(ValueName, out JsonToken valueToken))
                    {
                        var convert = option.ConverterProvider.Build(ValueType);
                        value = convert.FromToken(valueToken, option);
                    }
                    return CreateFunc(key, value);
                default:
                    throw new JsonException($"无法从{token.ValueType}转换为{Type},{this.GetType().Name}反序列化{Type}失败");
            }
        }

        //public override void ToWriter(JsonWriter writer, object obj, JsonOption option)
        //{
        //}
    }
}
