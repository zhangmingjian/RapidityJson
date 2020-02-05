using System;
using System.Collections.Generic;
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
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"无效的JSON Token，应为{{，实际为{reader.TokenType}", reader.Line, reader.Position);
            object instance = descriptor.CreateInstance();
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndObject: return instance;
                    case JsonTokenType.PropertyName:
                        var property = descriptor.GetMemberDefinition(reader.Value);
                        property?.SetValueMethod(instance, Deserialize(reader, property.MemberType));
                        break;
                }
            }
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
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException($"无效的JSON Token，应为[，实际为{reader.TokenType}", reader.Line, reader.Position);
            object instance = descriptor.CreateInstance();
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.EndArray: return instance;
                    case JsonTokenType.StartObject:
                        var item = ConvertObject(reader, new ObjectDescriptor(descriptor.ItemType));
                        descriptor.AddItemMethod(instance, item);
                        break;
                    case JsonTokenType.String:
                    case JsonTokenType.Number:
                    case JsonTokenType.True:
                    case JsonTokenType.False:
                    case JsonTokenType.Null:
                        var valueItem = ConvertValue(reader, descriptor.ItemType);
                        descriptor.AddItemMethod(instance, valueItem);
                        break;
                }
            }
            return instance;
        }

        public string Serialize(object obj)
        {
            return null;
        }

        public T Deserialize<T>(JsonReader reader)
        {
            return (T)Deserialize(reader, typeof(T));
        }
    }
}