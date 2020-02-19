using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Converters
{
    internal class StringConverter : TypeConverterBase, IConverterCreator
    {
        public StringConverter(Type type) : base(type) { }

        public bool CanConvert(Type type)
        {
            return type == typeof(string);
        }

        public ITypeConverter Create(Type type)
        {
            return new StringConverter(type);
        }

        public override object FromReader(JsonReader reader, JsonOption option)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.None: reader.Read(); return FromReader(reader, option);
                case JsonTokenType.String: return reader.Text;
                case JsonTokenType.Null: return null;
                default: throw new JsonException($"无效的JSON Token:{reader.TokenType},序列化对象:{Type}", reader.Line, reader.Position);
            }
        }

        public override object FromToken(JsonToken token, JsonOption option)
        {
            if (token.ValueType == JsonValueType.String)
            {
                return ((JsonString)token).Value;
            }
            if (token.ValueType == JsonValueType.Null)
            {
                return null;
            }
            throw new JsonException($"无法将{token.ValueType}转换为{Type},反序列化{Type}失败");
        }

        public override void ToWriter(JsonWriter writer, object value, JsonOption option)
        {
            writer.WriteString((string)value);
        }
    }

    internal class CharConverter : TypeConverterBase, IConverterCreator
    {
        public CharConverter(Type type) : base(type) { }

        public bool CanConvert(Type type)
        {
            return type == typeof(char) || type == typeof(char?);
        }

        public ITypeConverter Create(Type type)
        {
            return new CharConverter(type);
        }

        public override object FromReader(JsonReader reader, JsonOption option)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.None: reader.Read(); return FromReader(reader, option);
                case JsonTokenType.String:
                    if (char.TryParse(reader.Text, out char value))
                        return value;
                    throw new JsonException($"字符串{reader.Text}不是有效的{Type},反序列化{Type}失败", reader.Line, reader.Position);
                case JsonTokenType.Null:
                    if (Type == typeof(char?)) return null;
                    throw new JsonException($"无效的JSON Token,无法将null值赋给{Type}", reader.Line, reader.Position);
                default: throw new JsonException($"无效的JSON Token:{reader.TokenType},反序列化对象:{Type}", reader.Line, reader.Position);
            }
        }


        public override object FromToken(JsonToken token, JsonOption option)
        {
            switch (token.ValueType)
            {
                case JsonValueType.String:
                    var text = ((JsonString)token).Value;
                    if (char.TryParse(text, out char value))
                        return value;
                    throw new JsonException($"字符串{text}不是有效的{Type},反序列化{Type}失败");
                case JsonValueType.Null:
                    if (Type == typeof(char?)) return null;
                    throw new JsonException($"无效的JSON ValueType,无法将null值赋给{Type},反序列化{Type}失败");
                default: throw new JsonException($"无法将{token.ValueType}转换为{Type},反序列化{Type}失败");
            }
        }

        public override void ToWriter(JsonWriter writer, object value, JsonOption option)
        {
            if (value is char c)
                writer.WriteChar(c);
            else
            {
                var charNull = (char?)value;
                writer.WriteChar(charNull.Value);
            }
        }
    }
}
