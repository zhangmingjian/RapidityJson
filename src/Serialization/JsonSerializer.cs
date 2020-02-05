﻿using System;
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
            var desc = TypeDescriptor.Create(type);
            reader.Read();
            switch (desc.TypeKind)
            {
                case TypeKind.Object: return ConvertObject(reader, (ObjectDescriptor)desc);
                case TypeKind.Value: return ConvertValue(reader, type);
                case TypeKind.List: return ConvertList(reader, (EnumerableDescriptor)desc);
            }
            return null;
        }

        private object ConvertObject(JsonReader reader, ObjectDescriptor descriptor)
        {
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
                        property?.SetValue(instance, Deserialize(reader, property.MemberType));
                        break;
                }
            }
            while (reader.Read());
            return instance;
        }

        private object ConvertValue(JsonReader reader, Type type)
        {
            if (reader.TokenType != JsonTokenType.String
                && reader.TokenType != JsonTokenType.Number
                && reader.TokenType != JsonTokenType.True
                && reader.TokenType != JsonTokenType.False
                && reader.TokenType != JsonTokenType.Null)
                throw new JsonException($"无效的JSON Value Type:{reader.TokenType}", reader.Line, reader.Position);
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
                    var valueType = Nullable.GetUnderlyingType(type);
                    if (valueType != null)
                    {
                        if (reader.TokenType == JsonTokenType.Null) return null;
                        return ConvertValue(reader, valueType);
                    }
                    if (type == typeof(Guid))
                        return reader.GetGuid();
                    if (type.IsValueType)
                        return Activator.CreateInstance(type);
                    else return default;
            }
        }

        private object ConvertList(JsonReader reader, EnumerableDescriptor descriptor)
        {
            object instance = null;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartArray: instance = descriptor.CreateInstance(); break;
                    case JsonTokenType.EndArray: return instance;
                    case JsonTokenType.StartObject:
                        var item = ConvertObject(reader, new ObjectDescriptor(descriptor.ItemType));
                        descriptor.AddItem(instance, item);
                        break;
                    case JsonTokenType.String:
                    case JsonTokenType.Number:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                        var valueItem = ConvertValue(reader, descriptor.ItemType);
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

        public string Serialize(object obj)
        {
            if (obj == null)
                throw new JsonException("序列化对象的值不能为Null");
            using (var tw = new StringWriter())
            using (var write = new JsonWriter(tw))
            {
                Serialize(write, obj);
                return tw.ToString();
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
                case TypeKind.List: WriteList(writer, obj, (EnumerableDescriptor)desc); break;
                case TypeKind.Value: writer.WriteObject(obj); break;
            }
        }

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

        public T Deserialize<T>(JsonReader reader)
        {
            return (T)Deserialize(reader, typeof(T));
        }
    }
}