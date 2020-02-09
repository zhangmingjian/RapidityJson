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
                || type == typeof(DateTimeOffset))
                return true;
            return false;
        }

        public TypeConverter Create(Type type, TypeConverterProvider provider)
        {
            return new ValueConverter(type, provider);
        }

        public override object FromReader(JsonReader reader, JsonOption option)
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
                    return GetValue(reader, option);
                default: throw new JsonException($"无效的JSON Token:{reader.TokenType},序列化对象:{Type}", reader.Line, reader.Position);
            }
        }

        #region Get Value from reader

        private object GetValue(JsonReader reader, JsonOption option)
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
                        return convert.FromReader(reader, option);
                    }
                    throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
            }
        }

        private string GetString(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.String) return reader.Text;
            if (reader.TokenType == JsonTokenType.Null) return null;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private char GetChar(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.String && char.TryParse(reader.Text, out char value))
            {
                return value;
            }
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private byte GetByte(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && byte.TryParse(reader.Text, out byte value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private sbyte GetSByte(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && sbyte.TryParse(reader.Text, out sbyte value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private int GetInt(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && int.TryParse(reader.Text, out int value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private uint GetUInt(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && uint.TryParse(reader.Text, out uint value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private short GetShort(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && short.TryParse(reader.Text, out short value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private ushort GetUShort(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && ushort.TryParse(reader.Text, out ushort value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private long GetLong(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && long.TryParse(reader.Text, out long value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private ulong GetULong(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && ulong.TryParse(reader.Text, out ulong value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private float GetFloat(JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    if (float.TryParse(reader.Text, out float value))
                        return value;
                    break;
                case JsonTokenType.String:
                    if (float.TryParse(reader.Text, out float val) &&
                        (float.IsNaN(val)) || float.IsNegativeInfinity(val) || float.IsPositiveInfinity(val))
                    {
                        return val;
                    }
                    break;
                default: break;
            }
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private double GetDouble(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && double.TryParse(reader.Text, out double value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }
        private decimal GetDecimal(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && decimal.TryParse(reader.Text, out decimal value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private bool GetBoolean(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.True) return true;
            if (reader.TokenType == JsonTokenType.False) return false;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private Guid GetGuid(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.String && Guid.TryParse(reader.Text, out Guid value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private DBNull GetDBNull(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Null) return DBNull.Value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private DateTime GetDateTime(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.String && DateTime.TryParse(reader.Text, out DateTime value))
                return value;
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }

        #endregion

        public override object FromToken(JsonToken token, JsonOption option)
        {
            switch (Type.GetTypeCode(Type))
            {
                case TypeCode.Byte: return GetByte(token);
                case TypeCode.SByte: return GetSByte(token);
                case TypeCode.Int16: return GetShort(token);
                case TypeCode.UInt16: return GetUShort(token);
                case TypeCode.Int32: return GetInt(token);
                case TypeCode.UInt32: return GetUInt(token);
                case TypeCode.Int64: return GetLong(token);
                case TypeCode.UInt64: return GetULong(token);
                case TypeCode.Single: return GetFloat(token);
                case TypeCode.Double: return GetDouble(token);
                case TypeCode.Decimal: return GetDecimal(token);
                case TypeCode.String: return GetString(token);
                case TypeCode.Char: return GetChar(token);
                case TypeCode.Boolean: return GetBoolean(token);
                case TypeCode.DateTime: return GetDateTime(token);
                default:
                    if (Type == typeof(Guid)) return GetGuid(token);
                    if (Type == typeof(DBNull)) return GetDBNull(token);
                    var valueType = Nullable.GetUnderlyingType(Type);
                    if (valueType != null)
                    {
                        if (token.ValueType == JsonValueType.Null) return null;
                        var convert = Provider.Build(valueType);
                        return convert.FromToken(token, option);
                    }
                    throw new JsonException($"无法从{token.ValueType}转换为{Type},反序列化{Type}失败");
            }
        }

        #region  get value from jsontoken

        private object GetString(JsonToken token)
        {
            if (token.ValueType == JsonValueType.String)
            {
                return ((JsonString)token).Value;
            }
            if (token.ValueType == JsonValueType.Null)
            {
                return null;
            }
            return Throw(token);
        }

        private object GetChar(JsonToken token)
        {
            if (token.ValueType == JsonValueType.String
                && char.TryParse(((JsonString)token).Value, out char value))
            {
                return value;
            }
            return Throw(token);
        }

        private object GetBoolean(JsonToken token)
        {
            if (token.ValueType == JsonValueType.Boolean)
            {
                return ((JsonBoolean)token).Value;
            }
            return Throw(token);
        }

        private object GetByte(JsonToken token)
        {
            if (token.ValueType == JsonValueType.Number && ((JsonNumber)token).TryGetByte(out byte value))
            {
                return value;
            }
            return Throw(token);
        }

        private object GetSByte(JsonToken token)
        {
            if (token.ValueType == JsonValueType.Number && ((JsonNumber)token).TryGetSByte(out sbyte value))
            {
                return value;
            }
            return Throw(token);
        }

        private object GetInt(JsonToken token)
        {
            if (token.ValueType == JsonValueType.Number && ((JsonNumber)token).TryGetInt(out int value))
            {
                return value;
            }
            return Throw(token);
        }

        private object GetShort(JsonToken token)
        {
            if (token.ValueType == JsonValueType.Number && ((JsonNumber)token).TryGetShort(out short value))
            {
                return value;
            }
            return Throw(token);
        }

        private object GetUShort(JsonToken token)
        {
            if (token.ValueType == JsonValueType.Number && ((JsonNumber)token).TryGetUShort(out ushort value))
            {
                return value;
            }
            return Throw(token);
        }
        private object GetUInt(JsonToken token)
        {
            if (token.ValueType == JsonValueType.Number && ((JsonNumber)token).TryGetUInt(out uint value))
            {
                return value;
            }
            return Throw(token);
        }
        private object GetLong(JsonToken token)
        {
            if (token.ValueType == JsonValueType.Number && ((JsonNumber)token).TryGetLong(out long value))
            {
                return value;
            }
            return Throw(token);
        }

        private object GetULong(JsonToken token)
        {
            if (token.ValueType == JsonValueType.Number && ((JsonNumber)token).TryGetULong(out ulong value))
            {
                return value;
            }
            return Throw(token);
        }

        private object GetFloat(JsonToken token)
        {
            if (token.ValueType == JsonValueType.Number && ((JsonNumber)token).TryGetFloat(out float value))
            {
                return value;
            }
            return Throw(token);
        }
        private object GetDouble(JsonToken token)
        {
            if (token.ValueType == JsonValueType.Number && ((JsonNumber)token).TryGetDouble(out double value))
            {
                return value;
            }
            return Throw(token);
        }

        private object GetDecimal(JsonToken token)
        {
            if (token.ValueType == JsonValueType.Number && ((JsonNumber)token).TryGetDecimal(out decimal value))
            {
                return value;
            }
            return Throw(token);
        }

        private object GetGuid(JsonToken token)
        {
            if (token.ValueType == JsonValueType.String && Guid.TryParse(((JsonString)token).Value, out Guid value))
            {
                return value;
            }
            return Throw(token);
        }

        private object GetDBNull(JsonToken token)
        {
            if (token.ValueType == JsonValueType.Null)
            {
                return DBNull.Value;
            }
            return Throw(token);
        }

        private object GetDateTime(JsonToken token)
        {
            if (token.ValueType == JsonValueType.String && DateTime.TryParse(((JsonString)token).Value, out DateTime value))
            {
                return value;
            }
            return Throw(token);
        }

        private object Throw(JsonToken token)
        {
            throw new JsonException($"无法从{token.ValueType}转换为{Type},反序列化{Type}失败");
        }

        #endregion

        public override void WriteTo(JsonWriter writer, object value, JsonOption option)
        {
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
                    {
                        writer.WriteGuid((Guid)value);
                        break;
                    }
                    var valueType = Nullable.GetUnderlyingType(type);
                    if (valueType != null)
                    {
                        WriteTo(writer, Convert.ChangeType(value, valueType), option);
                        break;
                    }
                    throw new JsonException($"不支持的类型{value.GetType()}，{nameof(ValueConverter)}序列化失败");
            }
        }
    }
}
