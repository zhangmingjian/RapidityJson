using System;
using System.Collections.Generic;

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
                        return Activator.CreateInstance(Type, key, value);
                }
            } while (reader.Read());
            throw new JsonException($"无效的JSON Token: {reader.TokenType},序列化对象:{Type},应为:{JsonTokenType.StartObject} {{", reader.Line, reader.Position);
        }

        public override object FromToken(JsonToken token, JsonOption option)
        {
            throw new NotImplementedException();
        }

        //public override void ToWriter(JsonWriter writer, object obj, JsonOption option)
        //{
        //    writer.WriteStartObject();
        //    foreach (var member in this.MemberList)
        //    {
        //        var value = GetValue(obj, member.PropertyName);
        //        if (value == null && option.IgnoreNullValue)
        //            continue;
        //        if (HandleLoopReferenceValue(writer, member.PropertyName, value, option))
        //            continue;
        //        var name = option.CamelCaseNamed ? member.PropertyName.ToCamelCase() : member.PropertyName;
        //        writer.WritePropertyName(name);
        //        base.ToWriter(writer, value, option);
        //    }
        //    writer.WriteEndObject();
        //    option.LoopReferenceChecker.PopObject();
        //}
    }
}
