using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Converters
{
    internal class StringConverter : ITypeConverter, IConverterCreator
    {
        public Type Type { get; }

        public StringConverter(Type type)
        {
            this.Type = type;
        }

        public bool CanConvert(Type type)
        {
            return type == typeof(string);
        }

        public ITypeConverter Create(Type type)
        {
            return new StringConverter(type);
        }

        public object FromReader(JsonReader reader, JsonOption option)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.None: reader.Read(); return FromReader(reader, option);
                case JsonTokenType.String: return reader.Text;
                case JsonTokenType.Null: return null;
                default: throw new JsonException($"无效的JSON Token:{reader.TokenType},序列化对象:{Type}", reader.Line, reader.Position);
            }
        }

        public object FromElement(JsonElement element, JsonOption option)
        {
            if (element.ElementType == JsonElementType.String)
            {
                return ((JsonString)element).Value;
            }
            if (element.ElementType == JsonElementType.Null)
            {
                return null;
            }
            throw new JsonException($"无法将{element.ElementType}转换为{Type},反序列化{Type}失败");
        }

        public void ToWriter(JsonWriter writer, object value, JsonOption option)
        {
            writer.WriteString((string)value);
        }
    }
}
