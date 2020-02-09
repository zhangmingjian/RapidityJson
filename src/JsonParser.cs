using System;
using System.Reflection;

namespace Rapidity.Json
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JsonToken Parse(string json)
        {
            using (var reader = new JsonReader(json))
            {
                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.StartObject: return ReadObject(reader);
                        case JsonTokenType.StartArray: return ReadArray(reader);
                        case JsonTokenType.String: return new JsonString(reader.Text);
                        case JsonTokenType.Number: return new JsonNumber(reader.Number.Value);
                        case JsonTokenType.True: return new JsonBoolean(true);
                        case JsonTokenType.False: return new JsonBoolean(false);
                        case JsonTokenType.Null: return new JsonNull();
                        default: break;
                    }
                }
                throw new JsonException($"非法JSON Token:{reader.TokenType}", reader.Line, reader.Position);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JsonObject ParseObject(string json)
        {
            using (var reader = new JsonReader(json))
            {
                reader.Read();
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException($"不正确的JSON Object格式，缺失符号{{，实际为{reader.CurrentChar}", reader.Line, reader.Position);
                return ReadObject(reader);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public JsonArray ParseArray(string json)
        {
            using (var reader = new JsonReader(json))
            {
                reader.Read();
                if (reader.TokenType != JsonTokenType.StartArray)
                    throw new JsonException($"不正确的JSON Array格式，缺失符号[,实际为{reader.CurrentChar}", reader.Line, reader.Position);
                return ReadArray(reader);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public T To<T>(string json)
        {
            var type = typeof(T);
            T result = default;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Object:
                    result = (T)Activator.CreateInstance(type);
                    result = (T)ParseObject(new JsonReader(json), result);
                    break;
            }
            return result;
        }

        private object ParseObject(JsonReader reader, object data)
        {
            var type = data.GetType();
            PropertyInfo property = null;
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName:
                        property = type.GetProperty(reader.Text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.SetProperty);
                        break;
                    case JsonTokenType.String:
                        if (property != null)
                        {
                            property.SetValue(data, reader.Text);
                        }
                        break;
                }
            }
            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private JsonObject ReadObject(JsonReader reader)
        {
            var jObject = new JsonObject();
            string property = null;
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndObject: return jObject;
                    case JsonTokenType.PropertyName:
                        property = reader.Text;
                        break;
                    case JsonTokenType.String:
                        jObject.AddProperty(property, new JsonString(reader.Text));
                        break;
                    case JsonTokenType.Number:
                        jObject.AddProperty(property, new JsonNumber(reader.Number.Value));
                        break;
                    case JsonTokenType.True:
                        jObject.AddProperty(property, new JsonBoolean(true));
                        break;
                    case JsonTokenType.False:
                        jObject.AddProperty(property, new JsonBoolean(false));
                        break;
                    case JsonTokenType.Null:
                        jObject.AddProperty(property, new JsonNull());
                        break;
                    case JsonTokenType.StartObject:
                        jObject.AddProperty(property, ReadObject(reader));
                        break;
                    case JsonTokenType.StartArray:
                        jObject.AddProperty(property, ReadArray(reader));
                        break;
                }
            }
            return jObject;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private JsonArray ReadArray(JsonReader reader)
        {
            var jArray = new JsonArray();
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndArray: return jArray;
                    case JsonTokenType.StartArray:
                        jArray.Add(ReadArray(reader));
                        break;
                    case JsonTokenType.StartObject:
                        jArray.Add(ReadObject(reader));
                        break;
                    case JsonTokenType.String:
                        jArray.Add(new JsonString(reader.Text));
                        break;
                    case JsonTokenType.Number:
                        jArray.Add(new JsonNumber(reader.Number.Value));
                        break;
                    case JsonTokenType.True:
                        jArray.Add(new JsonBoolean(true));
                        break;
                    case JsonTokenType.False:
                        jArray.Add(new JsonBoolean(false));
                        break;
                    case JsonTokenType.Null:
                        jArray.Add(new JsonNull());
                        break;
                }
            }
            return jArray;
        }
    }
}
