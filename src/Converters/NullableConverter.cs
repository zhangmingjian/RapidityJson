using System;

namespace Rapidity.Json.Converters
{
    internal class NullableConverter : TypeConverterBase, IConverterCreator
    {
        public NullableConverter(Type type) : base(type) { }

        public bool CanConvert(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public ITypeConverter Create(Type type)
        {
            return new NullableConverter(Nullable.GetUnderlyingType(type));
        }

        public override object FromReader(JsonReader reader, JsonOption option)
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

        public override object FromToken(JsonToken token, JsonOption option)
        {
            if (token.ValueType == JsonValueType.Null) return null;
            var convert = option.ConverterProvider.Build(Type);
            return convert.FromToken(token, option);
        }

        public override void ToWriter(JsonWriter writer, object obj, JsonOption option)
        {
            var value = Convert.ChangeType(obj, Type);

            var convert = option.ConverterProvider.Build(Type);
            convert.ToWriter(writer, value, option);
        }
    }
}
