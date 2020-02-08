using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Converters
{
    internal class ValueConverter : TypeConverter, IConverterCreator
    {
        public ValueConverter(Type type, TypeConverterProvider provider) : base(type, provider)
        {
        }

        protected override Func<object> BuildCreateInstanceMethod(Type type)
        {
            object value;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean: value = default(bool); break;
                case TypeCode.Byte: value = default(byte); break;
                case TypeCode.Char: value = default(char); break;
                case TypeCode.Int16: value = default(Int16); break;
                case TypeCode.UInt16: value = default(UInt16); break;
                case TypeCode.Int32: value = default(Int32); break;
                case TypeCode.UInt32: value = default(UInt32); break;
                case TypeCode.Int64: value = default(Int64); break;
                case TypeCode.UInt64: value = default(UInt64); break;
                case TypeCode.Single: value = default(Single); break;
                case TypeCode.Double: value = default(double); break;
                case TypeCode.Decimal: value = default(decimal); break;
                case TypeCode.SByte: value = default(sbyte); break;
                case TypeCode.DateTime: value = default(DateTime); break;
                case TypeCode.String: value = default(string); break;
                case TypeCode.DBNull: value = default(DBNull); break;
                default:
                    if (type.IsValueType)
                        value = Activator.CreateInstance(type);
                    else
                        value = default; break;
            }
            return () => value;
        }

        public bool CanConvert(Type type)
        {
            //The primitive types are Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, 
            //Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single.
            if (type.IsPrimitive
                || Nullable.GetUnderlyingType(type) != null
                || type == typeof(DBNull)
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(Guid)
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type.IsEnum)
                return true;
            return false;
        }

        public TypeConverter Create(Type type, TypeConverterProvider provider)
        {
            return new ValueConverter(type, provider);
        }

        public override object FromReader(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.None)
                reader.Read();
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                case JsonTokenType.False:
                case JsonTokenType.String:
                case JsonTokenType.Number:
                case JsonTokenType.Null:
                    return GetValue(reader);
                default: throw new JsonException($"无效的JSON Value Type:{reader.TokenType},序列化对象:{Type}", reader.Line, reader.Position);
            }
        }

        public override object FromToken(JsonToken token)
        {
            throw new NotImplementedException();
        }

        #region Get Value

        private object GetValue(JsonReader reader)
        {
            switch (Type.GetTypeCode(Type))
            {
                case TypeCode.Boolean: return GetBoolean(reader);
                case TypeCode.Int32: return GetInt(reader);
                case TypeCode.Int16: return GetShort(reader);
                case TypeCode.UInt16: return GetUShort(reader);
                case TypeCode.UInt32: return GetUInt(reader);
                case TypeCode.UInt64: return GetULong(reader);
                case TypeCode.Int64: return GetLong(reader);
                case TypeCode.Single: return GetFloat(reader);
                case TypeCode.Double: return GetDouble(reader);
                case TypeCode.Decimal: return GetDecimal(reader);
                case TypeCode.String: return GetString(reader);
                case TypeCode.DateTime: return GetDateTime(reader);
                case TypeCode.Char: return GetChar(reader);
                case TypeCode.Byte: return GetByte(reader);
                case TypeCode.SByte: return GetSByte(reader);
                default:
                    if (Type == typeof(Guid)) return GetGuid(reader);
                    if (Type == typeof(DBNull)) return GetDBNull(reader);
                    var valueType = Nullable.GetUnderlyingType(Type);
                    if (valueType != null)
                    {
                        if (reader.TokenType == JsonTokenType.Null) return null;
                        var convert = Provider.Build(valueType);
                        return convert.FromReader(reader);
                    }
                    throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
            }
        }

        private string GetString(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.String) return reader.Value;
            if (reader.TokenType == JsonTokenType.Null) return null;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private char GetChar(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.String && char.TryParse(reader.Value, out char value))
            {
                return value;
            }
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private byte GetByte(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && byte.TryParse(reader.Value, out byte value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private sbyte GetSByte(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && sbyte.TryParse(reader.Value, out sbyte value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private int GetInt(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && int.TryParse(reader.Value, out int value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private uint GetUInt(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && uint.TryParse(reader.Value, out uint value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private short GetShort(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && short.TryParse(reader.Value, out short value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private ushort GetUShort(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && ushort.TryParse(reader.Value, out ushort value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private long GetLong(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && long.TryParse(reader.Value, out long value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private ulong GetULong(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && ulong.TryParse(reader.Value, out ulong value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private float GetFloat(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && float.TryParse(reader.Value, out float value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private double GetDouble(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && double.TryParse(reader.Value, out double value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
        }
        private decimal GetDecimal(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && decimal.TryParse(reader.Value, out decimal value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private bool GetBoolean(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.True) return true;
            if (reader.TokenType == JsonTokenType.False) return false;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private Guid GetGuid(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.String && Guid.TryParse(reader.Value, out Guid value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private DBNull GetDBNull(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Null) return DBNull.Value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private DateTime GetDateTime(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.String && DateTime.TryParse(reader.Value, out DateTime value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Value},序列化对象:{Type}", reader.Line, reader.Position);
        }

        #endregion

        public override void WriteTo(JsonWriter writer, object value)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            var type = value.GetType();
            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.String: writer.WriteString((string)value); break;
                case TypeCode.Char: writer.WriteChar((char)value); break;
                case TypeCode.Boolean: writer.WriteBoolean((bool)value); break;
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32: writer.WriteInt((int)value); break;
                case TypeCode.UInt32: writer.WriteUInt((uint)value); break;
                case TypeCode.Int64: writer.WriteLong((long)value); break;
                case TypeCode.UInt64: writer.WriteULong((ulong)value); break;
                case TypeCode.Single: writer.WriteFloat((float)value); break;
                case TypeCode.Double: writer.WriteDouble((double)value); break;
                case TypeCode.Decimal: writer.WriteDecimal((decimal)value); break;
                case TypeCode.DateTime: writer.WriteDateTime((DateTime)value); break;
                case TypeCode.Empty:
                case TypeCode.DBNull: writer.WriteNull(); break;
                case TypeCode.Object:
                    if (type == typeof(Guid))
                        writer.WriteGuid((Guid)value);
                    else
                    {
                        var valueType = Nullable.GetUnderlyingType(type);
                        if (valueType != null)
                        {
                            WriteTo(writer, Convert.ChangeType(value, valueType));
                            break;
                        }
                    }
                    break;
            }
        }
    }
}
