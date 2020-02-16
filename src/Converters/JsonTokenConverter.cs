using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Converters
{
    internal class JsonTokenConverter : TypeConverterBase, IConverterCreator
    {
        public JsonTokenConverter(Type type) : base(type)
        {
        }

        public bool CanConvert(Type type)
        {
            return type == typeof(object) || typeof(JsonToken).IsAssignableFrom(type);
        }

        public ITypeConverter Create(Type type)
        {
            return new JsonTokenConverter(type);
        }

        public override object FromReader(JsonReader reader, JsonOption option)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.None:
                    reader.Read();
                    return FromReader(reader, option);
                case JsonTokenType.StartObject: return ReadObject(reader);
                case JsonTokenType.StartArray: return ReadArray(reader);
                case JsonTokenType.String: return reader.Text;
                case JsonTokenType.Number: return reader.Number.Value;
                case JsonTokenType.True: return true;
                case JsonTokenType.False: return false;
                case JsonTokenType.Null: return null;
                default: throw new JsonException($"无效的JSON Token: {reader.TokenType},序列化对象:{Type},应为：{JsonTokenType.StartArray}[", reader.Line, reader.Position);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private JsonObject ReadObject(JsonReader reader)
        {
            JsonObject token = new JsonObject();
            string property = null;
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndObject: return token;
                    case JsonTokenType.PropertyName:
                        property = reader.Text;
                        break;
                    case JsonTokenType.StartObject:
                        token.AddProperty(property, ReadObject(reader));
                        break;
                    case JsonTokenType.StartArray:
                        token.AddProperty(property, ReadArray(reader));
                        break;
                    case JsonTokenType.String:
                        token.AddProperty(property, reader.Text);
                        break;
                    case JsonTokenType.Number:
                        token.AddProperty(property, reader.Number.Value);
                        break;
                    case JsonTokenType.True:
                        token.AddProperty(property, true);
                        break;
                    case JsonTokenType.False:
                        token.AddProperty(property, false);
                        break;
                    case JsonTokenType.Null:
                        token.AddProperty(property, null);
                        break;
                }
            }
            return token;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private JsonArray ReadArray(JsonReader reader)
        {
            JsonArray token = new JsonArray();
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndArray: return token;
                    case JsonTokenType.StartArray:
                        token.Add(ReadArray(reader));
                        break;
                    case JsonTokenType.StartObject:
                        token.Add(ReadObject(reader));
                        break;
                    case JsonTokenType.String:
                        token.Add(reader.Text);
                        break;
                    case JsonTokenType.Number:
                        token.Add(reader.Number.Value);
                        break;
                    case JsonTokenType.True:
                        token.Add(true);
                        break;
                    case JsonTokenType.False:
                        token.Add(false);
                        break;
                    case JsonTokenType.Null:
                        token.Add(null);
                        break;
                }
            }
            return token;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override object FromToken(JsonToken token, JsonOption option)
        {
            if (typeof(JsonToken).IsAssignableFrom(Type))
                return token;
            if (Type == typeof(object))
            {
                switch (token.ValueType)
                {
                    case JsonValueType.String: return ((JsonString)token).Value;
                    case JsonValueType.Boolean: return ((JsonBoolean)token).Value;
                    case JsonValueType.Null: return null;
                    case JsonValueType.Number:
                        var number = (JsonNumber)token;
                        if (number.TryGetInt(out int intVal)) return intVal;
                        if (number.TryGetLong(out long longVal)) return longVal;
                        if (number.TryGetFloat(out float floatVal)) return floatVal;
                        if (number.TryGetDouble(out double doubleVal)) return doubleVal;
                        if (number.TryGetDecimal(out decimal decimalVal)) return decimalVal;
                        break;
                    default: return token;
                }
            }
            var convert = option.ConverterProvider.Build(Type);
            return convert.FromToken(token, option);
        }

        public override void ToWriter(JsonWriter writer, object obj, JsonOption option)
        {
            if (obj is JsonToken token)
            {
                writer.WriteToken(token);
                return;
            }
            writer.WriteString(obj.ToString());
        }
    }
}
