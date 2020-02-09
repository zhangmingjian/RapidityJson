using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Converters
{
    internal class JsonTokenConverter : TypeConverter, IConverterCreator
    {
        public JsonTokenConverter(Type type, TypeConverterProvider provider) : base(type, provider)
        {
        }

        public bool CanConvert(Type type)
        {
            return type == typeof(object) || type == typeof(JsonToken) || typeof(JsonToken).IsAssignableFrom(type);
        }

        public TypeConverter Create(Type type, TypeConverterProvider provider)
        {
            return new JsonTokenConverter(type, provider);
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
                case JsonTokenType.String: return new JsonString(reader.Text);
                case JsonTokenType.Number: return new JsonNumber(reader.Text);
                case JsonTokenType.True: return new JsonBoolean(true);
                case JsonTokenType.False: return new JsonBoolean(false);
                case JsonTokenType.Null: return new JsonNull();
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
                        token.AddProperty(property, new JsonString(reader.Text));
                        break;
                    case JsonTokenType.Number:
                        token.AddProperty(property, new JsonNumber(reader.Number.Value));
                        break;
                    case JsonTokenType.True:
                        token.AddProperty(property, new JsonBoolean(true));
                        break;
                    case JsonTokenType.False:
                        token.AddProperty(property, new JsonBoolean(false));
                        break;
                    case JsonTokenType.Null:
                        token.AddProperty(property, new JsonNull());
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
                        token.Add(new JsonString(reader.Text));
                        break;
                    case JsonTokenType.Number:
                        token.Add(new JsonNumber(reader.Text));
                        break;
                    case JsonTokenType.True:
                        token.Add(new JsonBoolean(true));
                        break;
                    case JsonTokenType.False:
                        token.Add(new JsonBoolean(false));
                        break;
                    case JsonTokenType.Null:
                        token.Add(new JsonNull());
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
            if (Type == typeof(object)
                || typeof(JsonToken).IsAssignableFrom(Type))
                return token;
            var convert = Provider.Build(Type);
            return convert.FromToken(token, option);
        }

        public override void WriteTo(JsonWriter writer, object obj, JsonOption option)
        {
            var token = obj as JsonToken;
            if (token != null)
            {
                writer.WriteToken(token);
                return;
            }
            throw new JsonException($"不支持的类型{obj.GetType()}，{nameof(JsonTokenConverter)}序列化失败");
        }
    }
}
