using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Converters
{
    internal class JsonElementConverter : TypeConverterBase, IConverterCreator
    {
        public JsonElementConverter(Type type) : base(type)
        {
        }

        public bool CanConvert(Type type)
        {
            return typeof(JsonElement).IsAssignableFrom(type) || type == typeof(object);
        }

        public ITypeConverter Create(Type type)
        {
            return new JsonElementConverter(type);
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
        public override object FromElement(JsonElement element, JsonOption option)
        {
            if (typeof(JsonElement).IsAssignableFrom(Type))
                return element;
            if (Type == typeof(object))
            {
                switch (element.ElementType)
                {
                    case JsonElementType.String: return ((JsonString)element).Value;
                    case JsonElementType.Boolean: return ((JsonBoolean)element).Value;
                    case JsonElementType.Null: return null;
                    case JsonElementType.Number:
                        var number = (JsonNumber)element;
                        if (number.TryGetInt(out int intVal)) return intVal;
                        if (number.TryGetLong(out long longVal)) return longVal;
                        if (number.TryGetFloat(out float floatVal)) return floatVal;
                        if (number.TryGetDouble(out double doubleVal)) return doubleVal;
                        if (number.TryGetDecimal(out decimal decimalVal)) return decimalVal;
                        break;
                    default: return element;
                }
            }
            var convert = option.ConverterProvider.Build(Type);
            return convert.FromElement(element, option);
        }

        public override void ToWriter(JsonWriter writer, object obj, JsonOption option)
        {
            if (obj is JsonElement element)
            {
                WriteToken(writer, element, option);
                return;
            }
            writer.WriteString(obj.ToString());
        }

        public void WriteToken(JsonWriter writer, JsonElement element, JsonOption option)
        {
            switch (element.ElementType)
            {
                case JsonElementType.Object: WriteObject(writer, (JsonObject)element, option); break;
                case JsonElementType.Array: WriteArray(writer, (JsonArray)element, option); break;
                case JsonElementType.String:
                    writer.WriteString((JsonString)element); break;
                case JsonElementType.Number: writer.WriteNumber((JsonNumber)element); break;
                case JsonElementType.Boolean: writer.WriteBoolean((JsonBoolean)element); break;
                case JsonElementType.Null: writer.WriteNull(); break;
                default: break;
            }
        }

        private void WriteObject(JsonWriter writer, JsonObject token, JsonOption option)
        {
            writer.WriteStartObject();
            foreach (var property in token.GetAllProperty())
            {
                if (HandleLoopReferenceValue(writer, property.Name, property.Value, option))
                    continue;
                writer.WritePropertyName(property.Name);
                WriteToken(writer, property.Value, option);
            }
            writer.WriteEndObject();
            option.LoopReferenceChecker.PopObject();
        }

        private void WriteArray(JsonWriter writer, JsonArray token, JsonOption option)
        {
            writer.WriteStartArray();
            foreach (var item in token)
            {
                if (HandleLoopReferenceValue(writer, item, option))
                    continue;
                WriteToken(writer, item, option);
            }
            writer.WriteEndArray();
            option.LoopReferenceChecker.PopObject();
        }
    }
}
