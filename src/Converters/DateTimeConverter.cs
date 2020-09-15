using System;

namespace Rapidity.Json.Converters
{
    internal class DateTimeConverter : ITypeConverter, IConverterCreator
    {
        public Type Type { get; }

        public DateTimeConverter(Type type)  
        {
            Type = type;
        }

        public bool CanConvert(Type type)
        {
            return type == typeof(DateTime) || type == typeof(DateTimeOffset);
        }

        public ITypeConverter Create(Type type)
        {
            return new DateTimeConverter(type);
        }

        public object FromReader(JsonReader reader, JsonOption option)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.None:
                    reader.Read();
                    return FromReader(reader, option);
                case JsonTokenType.String:
                    if (Type == typeof(DateTime) && DateTime.TryParse(reader.Text, out DateTime value))
                        return value;
                    if (Type == typeof(DateTimeOffset) && DateTimeOffset.TryParse(reader.Text, out DateTimeOffset val))
                        return val;
                    throw new JsonException($"无法将JsonTokenType.String:{reader.Text}转换为{Type},反序列化{Type}失败", reader.Line, reader.Position);
                //case JsonTokenType.Number:
                default: throw new JsonException($"无效的JSON Token:{reader.TokenType},反序列化{Type}失败", reader.Line, reader.Position);
            }
        }

        public object FromElement(JsonElement element, JsonOption option)
        {
            switch (element.ElementType)
            {
                case JsonElementType.String:
                    var text = ((JsonString)element).Value;
                    if (Type == typeof(DateTime) && DateTime.TryParse(text, out DateTime value))
                        return value;
                    if (Type == typeof(DateTimeOffset) && DateTimeOffset.TryParse(text, out DateTimeOffset val))
                        return val;
                    throw new JsonException($"无法将JsonValueType.String:{text}转换为{Type}，反序列化{Type}失败");
                default: throw new JsonException($"无效的JSON Token:{element.ElementType},反序列化{Type}失败");
            }
        }

        public void ToWriter(JsonWriter writer, object obj, JsonOption option)
        {
            if (obj is DateTime dateTime)
                writer.WriteDateTime(dateTime);
            else 
                writer.WriteDateTimeOffset((DateTimeOffset)obj);
        }
    }
}
