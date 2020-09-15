using System;

namespace Rapidity.Json.Converters
{
    internal class NullableConverter : ITypeConverter, IConverterCreator
    {
        public Type Type { get; }

        public NullableConverter(Type type)
        {
            Type = type;
        }

        public bool CanConvert(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public ITypeConverter Create(Type type)
        {
            return new NullableConverter(Nullable.GetUnderlyingType(type));
        }

        public object FromReader(JsonReader reader, JsonOption option)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.None:
                    reader.Read();
                    return FromReader(reader, option);
                case JsonTokenType.Null: return null;
                default:
                    var convert = option.ConverterProvider.Build(Type);
                    return convert.FromReader(reader, option);
            }
        }

        public object FromElement(JsonElement element, JsonOption option)
        {
            if (element.ElementType == JsonElementType.Null) return null;
            var convert = option.ConverterProvider.Build(Type);
            return convert.FromElement(element, option);
        }

        public void ToWriter(JsonWriter writer, object obj, JsonOption option)
        {
            var convert = option.ConverterProvider.Build(Type);
            convert.ToWriter(writer, obj, option);
        }
    }
}
