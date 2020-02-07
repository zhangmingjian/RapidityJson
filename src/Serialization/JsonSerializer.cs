using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rapidity.Json.Serialization
{
    public class JsonSerializer
    {
        public object Deserialize(JsonReader reader, Type type)
        {
            reader.Read();
            return Convert(reader, type);
        }

        #region convert

        private object Convert(JsonReader reader, Type type)
        {
            var desc = TypeDescriptor.Create(type);
            switch (desc.TypeKind)
            {
                case TypeKind.Object: return ConvertObject(reader, (ObjectDescriptor)desc);
                case TypeKind.Value: return ConvertValue(reader, type);
                case TypeKind.List: return ConvertList(reader, (EnumerableDescriptor)desc);
                case TypeKind.Array: return ConvertArray(reader, (ArrayDescriptor)desc);
                case TypeKind.Dictionary: return ConvertDictionary(reader, (DictionaryDescriptor)desc);
            }
            return null;
        }

        private object ConvertObject(JsonReader reader, ObjectDescriptor descriptor)
        {
            if (reader.TokenType != JsonTokenType.StartArray && reader.TokenType != JsonTokenType.Null)
                throw new JsonException($"无效的JSON Token: {reader.TokenType},序列化对象:{descriptor.Type}, 应为{JsonTokenType.StartObject} {{", reader.Line, reader.Position);
            object instance = null;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject: instance = descriptor.CreateInstance(); break;
                    case JsonTokenType.Null:
                    case JsonTokenType.EndObject: return instance;
                    case JsonTokenType.PropertyName:
                        var property = descriptor.GetMemberDefinition(reader.Value);
                        reader.Read();
                        property?.SetValue(instance, Convert(reader, property.MemberType));
                        break;
                    default: throw new JsonException($"无效的JSON Token:{reader.TokenType}", reader.Line, reader.Position);
                }
            }
            while (reader.Read());
            return instance;
        }

        private object ConvertValue(JsonReader reader, Type type)
        {
            if (reader.TokenType == JsonTokenType.StartObject
                || reader.TokenType == JsonTokenType.EndObject
                || reader.TokenType == JsonTokenType.StartArray
                || reader.TokenType == JsonTokenType.EndArray
                || reader.TokenType == JsonTokenType.PropertyName)
                throw new JsonException($"无效的JSON Value Type:{reader.TokenType},序列化对象:{type}", reader.Line, reader.Position);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean: return reader.GetBoolean();
                case TypeCode.Int32: return reader.GetInt();
                case TypeCode.Int16: return reader.GetShort();
                case TypeCode.UInt16: return reader.GetUShort();
                case TypeCode.UInt32: return reader.GetUInt();
                case TypeCode.UInt64: return reader.GetULong();
                case TypeCode.Int64: return reader.GetLong();
                case TypeCode.Single: return reader.GetFloat();
                case TypeCode.Double: return reader.GetDouble();
                case TypeCode.Decimal: return reader.GetDecimal();
                case TypeCode.String: return reader.GetString();
                case TypeCode.DateTime: return reader.GetDateTime();
                case TypeCode.Char: return reader.GetChar();
                case TypeCode.Byte: return reader.GetByte();
                case TypeCode.SByte: return reader.GetSByte();
                default:
                    if (type == typeof(Guid))
                        return reader.GetGuid();
                    var valueType = Nullable.GetUnderlyingType(type);
                    if (valueType != null)
                    {
                        if (reader.TokenType == JsonTokenType.Null) return null;
                        return ConvertValue(reader, valueType);
                    }
                    if (type.IsValueType)
                        return Activator.CreateInstance(type);
                    else return default;
            }
        }

        private object ConvertList(JsonReader reader, EnumerableDescriptor descriptor)
        {
            if (reader.TokenType != JsonTokenType.StartArray && reader.TokenType != JsonTokenType.Null)
                throw new JsonException($"无效的JSON Token: {reader.TokenType},,序列化对象:{descriptor.Type},应为：{JsonTokenType.StartArray}[", reader.Line, reader.Position);
            object instance = null;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndArray: return instance;
                    case JsonTokenType.StartArray:
                        if (instance == null) instance = descriptor.CreateInstance();
                        else
                            descriptor.AddItem(instance, Convert(reader, descriptor.ItemType));
                        break;
                    case JsonTokenType.StartObject:
                        descriptor.AddItem(instance, Convert(reader, descriptor.ItemType));
                        break;
                    case JsonTokenType.String:
                    case JsonTokenType.Number:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                        var valueItem = Convert(reader, descriptor.ItemType);
                        descriptor.AddItem(instance, valueItem);
                        break;
                    case JsonTokenType.Null:
                        if (instance == null) return instance;
                        descriptor.AddItem(instance, null);
                        break;
                }
            } while (reader.Read());
            return instance;
        }

        private object ConvertArray(JsonReader reader, ArrayDescriptor descriptor)
        {
            var instance = ConvertList(reader, descriptor);
            return descriptor.ToArray(instance);
        }

        private object ConvertDictionary(JsonReader reader, DictionaryDescriptor descriptor)
        {
            if (reader.TokenType != JsonTokenType.StartObject 
                && reader.TokenType != JsonTokenType.Null)
                throw new JsonException($"无效的JSON Token: {reader.TokenType},序列化对象:{descriptor.Type},应为:{JsonTokenType.StartObject} {{", reader.Line, reader.Position);
            object instance = null;
            object key = null;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndObject: return instance;
                    case JsonTokenType.StartObject:
                        if (instance == null) instance = descriptor.CreateInstance();
                        else
                            descriptor.SetKeyValue(instance, key, Convert(reader, descriptor.ValueType));
                        break;
                    case JsonTokenType.PropertyName:
                        key = reader.Value;
                        break;
                    case JsonTokenType.StartArray:
                    case JsonTokenType.String:
                    case JsonTokenType.Number:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                    case JsonTokenType.Null:
                        descriptor.SetKeyValue(instance, key, Convert(reader, descriptor.ValueType));
                        break;
                }
            } while (reader.Read());
            return instance;
        }
        #endregion

        public string Serialize(object obj)
        {
            if (obj == null) throw new JsonException("序列化对象的值不能为Null");
            using (var sw = new StringWriter())
            using (var write = new JsonWriter(sw))
            {
                Serialize(write, obj);
                return sw.ToString();
            }
        }

        public void Serialize(JsonWriter writer, object obj)
        {
            if (obj == null)
            {
                writer.WriteNull();
                return;
            }
            var desc = TypeDescriptor.Create(obj.GetType());
            switch (desc.TypeKind)
            {
                case TypeKind.Object: WriteObject(writer, obj, (ObjectDescriptor)desc); break;
                case TypeKind.Value: writer.WriteValue(obj); break;
                case TypeKind.List: WriteList(writer, obj, (EnumerableDescriptor)desc); break;
                case TypeKind.Dictionary: WriteDictionary(writer, obj, (DictionaryDescriptor)desc); break;
            }
        }

        #region write
        private void WriteObject(JsonWriter writer, object obj, ObjectDescriptor descriptor)
        {
            writer.WriteStartObject();
            foreach (var member in descriptor.MemberDefinitions)
            {
                writer.WritePropertyName(member.JsonProperty);
                var value = member.GetValue(obj);
                Serialize(writer, value);
            }
            writer.WriteEndObject();
        }

        private void WriteList(JsonWriter writer, object list, EnumerableDescriptor descriptor)
        {
            writer.WriteStartArray();
            var enumer = descriptor.GetEnumerator(list);
            while (enumer.MoveNext())
            {
                Serialize(writer, enumer.Current);
            }
            writer.WriteEndArray();
        }

        private void WriteDictionary(JsonWriter writer, object dic, DictionaryDescriptor descriptor)
        {
            writer.WriteStartObject();
            var keys = descriptor.GetKeys(dic);
            while (keys.MoveNext())
            {
                writer.WritePropertyName(keys.Current.ToString());
                var value = descriptor.GetValue(dic, keys.Current);
                Serialize(writer, value);
            }
            writer.WriteEndObject();
        }

        #endregion

        public T Deserialize<T>(JsonReader reader)
        {
            return (T)Deserialize(reader, typeof(T));
        }
    }
}