using System;

namespace Rapidity.Json.Converters
{
    internal class ValueConverter : TypeConverterBase, IConverterCreator
    {
        public ValueConverter(Type type) : base(type)
        {
        }

        public bool CanConvert(Type type)
        {
            //The primitive types are Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, 
            //Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single.
            return type.IsPrimitive
                     || type == typeof(DBNull)
                     || type == typeof(decimal)
                     || type == typeof(Guid);
        }

        public ITypeConverter Create(Type type)
        {
            return new ValueConverter(type);
        }

        public override object FromReader(JsonReader reader, JsonOption option)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.None:
                    reader.Read();
                    return FromReader(reader, option);
                case JsonTokenType.True:
                    if (Type == typeof(bool)) return true;
                    break;
                case JsonTokenType.False:
                    if (Type == typeof(bool)) return false;
                    break;
                case JsonTokenType.String:
                    if (Type == typeof(Guid) && Guid.TryParse(reader.Text, out Guid guidVal)) return guidVal;
                    if (Type == typeof(char) && char.TryParse(reader.Text, out char charVal)) return charVal;
                    if (Type == typeof(float) && reader.Text == float.NaN.ToString()) return float.NaN;
                    if (Type == typeof(float) && reader.Text == float.NegativeInfinity.ToString()) return float.NegativeInfinity;
                    if (Type == typeof(float) && reader.Text == float.PositiveInfinity.ToString()) return float.PositiveInfinity;
                    if (Type == typeof(double) && reader.Text == double.NaN.ToString()) return double.NaN;
                    if (Type == typeof(double) && reader.Text == double.NegativeInfinity.ToString()) return double.NegativeInfinity;
                    if (Type == typeof(double) && reader.Text == double.PositiveInfinity.ToString()) return double.PositiveInfinity;
                    break;
                case JsonTokenType.Null:
                    if (Type == typeof(DBNull)) return DBNull.Value;
                    break;
                case JsonTokenType.Number:
                    if (Type == typeof(int) && int.TryParse(reader.Text, out int intVal)) return intVal;
                    else if (Type == typeof(long) && long.TryParse(reader.Text, out long longVal)) return longVal;
                    else if (Type == typeof(float) && float.TryParse(reader.Text, out float floatVal)) return floatVal;
                    else if (Type == typeof(double) && double.TryParse(reader.Text, out double doubleVal)) return doubleVal;
                    else if (Type == typeof(decimal) && decimal.TryParse(reader.Text, out decimal decimalVal)) return decimalVal;
                    else if (Type == typeof(byte) && byte.TryParse(reader.Text, out byte byteVal)) return byteVal;
                    else if (Type == typeof(sbyte) && sbyte.TryParse(reader.Text, out sbyte sbyteVal)) return sbyteVal;
                    else if (Type == typeof(short) && short.TryParse(reader.Text, out short shrotVal)) return shrotVal;
                    else if (Type == typeof(ushort) && ushort.TryParse(reader.Text, out ushort ushortVal)) return ushortVal;
                    else if (Type == typeof(uint) && uint.TryParse(reader.Text, out uint uintVal)) return uintVal;
                    else if (Type == typeof(ulong) && ulong.TryParse(reader.Text, out ulong ulongVal)) return ulongVal;
                    break;
            }
            throw new JsonException($"无效的JSON Token:{reader.TokenType},序列化对象:{Type}", reader.Line, reader.Position);
        }

        #region Get Value from reader

        private object GetValue(JsonReader reader, JsonOption option)
        {
            //switch (Type.GetTypeCode(Type))
            //{
            //    case TypeCode.Boolean: return GetBoolean(reader);
            //    case TypeCode.Int32: return GetInt(reader);
            //    case TypeCode.Int16: return GetShort(reader);
            //    case TypeCode.UInt16: return GetUShort(reader);
            //    case TypeCode.UInt32: return GetUInt(reader);
            //    case TypeCode.UInt64: return GetULong(reader);
            //    case TypeCode.Int64: return GetLong(reader);
            //    case TypeCode.Single: return GetFloat(reader);
            //    case TypeCode.Double: return GetDouble(reader);
            //    case TypeCode.Decimal: return GetDecimal(reader);
            //    case TypeCode.Char: return GetChar(reader);
            //    case TypeCode.Byte: return GetByte(reader);
            //    case TypeCode.SByte: return GetSByte(reader);
            //    default:
            //        if (Type == typeof(Guid)) return GetGuid(reader);
            //        if (Type == typeof(DBNull)) return GetDBNull(reader);
            //        //var valueType = Nullable.GetUnderlyingType(Type);
            //        //if (valueType != null)
            //        //{
            //        //    if (reader.TokenType == JsonTokenType.Null) return null;
            //        //    var convert = option.ConverterProvider.Build(valueType);
            //        //    return convert.FromReader(reader, option);
            //        //}
            //        throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
            //}

            if (Type == typeof(int)) return GetInt(reader);
            else if (Type == typeof(long)) return GetLong(reader);
            else if (Type == typeof(bool)) return GetBoolean(reader);
            else if (Type == typeof(float)) return GetFloat(reader);
            else if (Type == typeof(double)) return GetDouble(reader);
            else if (Type == typeof(decimal)) return GetDecimal(reader);
            else if (Type == typeof(Guid)) return GetGuid(reader);
            else if (Type == typeof(byte)) return GetByte(reader);
            else if (Type == typeof(sbyte)) return GetSByte(reader);
            else if (Type == typeof(short)) return GetShort(reader);
            else if (Type == typeof(ushort)) return GetUShort(reader);
            else if (Type == typeof(uint)) return GetUInt(reader);
            else if (Type == typeof(ulong)) return GetULong(reader);
            else if (Type == typeof(char)) return GetChar(reader);
            else if (Type == typeof(DBNull)) return GetDBNull(reader);
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
                    if (reader.Text == float.NaN.ToString()) return float.NaN;
                    if (reader.Text == float.NegativeInfinity.ToString()) return float.NegativeInfinity;
                    if (reader.Text == float.PositiveInfinity.ToString()) return float.PositiveInfinity;
                    break;
            }
            throw new JsonException($"无效的JSON Token:{reader.TokenType} {reader.Text},序列化对象:{Type}", reader.Line, reader.Position);
        }

        private double GetDouble(JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number && double.TryParse(reader.Text, out double value))
                return value;
            if (reader.TokenType == JsonTokenType.String)
            {
                if (reader.Text == double.NaN.ToString()) return double.NaN;
                if (reader.Text == double.NegativeInfinity.ToString()) return double.NegativeInfinity;
                if (reader.Text == double.PositiveInfinity.ToString()) return double.PositiveInfinity;
            }
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

        #endregion

        public override object FromToken(JsonToken token, JsonOption option)
        {
            switch (token.ValueType)
            {
                case JsonValueType.Boolean:
                    if (Type == typeof(bool)) return ((JsonBoolean)token).Value;
                    break;
                case JsonValueType.String:
                    var strToken = (JsonString)token;
                    if (Type == typeof(Guid) && Guid.TryParse(strToken.Value, out Guid guidVal)) return guidVal;
                    if (Type == typeof(char) && char.TryParse(strToken.Value, out char charVal)) return charVal;
                    if (Type == typeof(float) && strToken.Value == float.NaN.ToString()) return float.NaN;
                    if (Type == typeof(float) && strToken.Value == float.NegativeInfinity.ToString()) return float.NegativeInfinity;
                    if (Type == typeof(float) && strToken.Value == float.PositiveInfinity.ToString()) return float.PositiveInfinity;
                    if (Type == typeof(double) && strToken.Value == double.NaN.ToString()) return double.NaN;
                    if (Type == typeof(double) && strToken.Value == double.NegativeInfinity.ToString()) return double.NegativeInfinity;
                    if (Type == typeof(double) && strToken.Value == double.PositiveInfinity.ToString()) return double.PositiveInfinity;
                    break;
                case JsonValueType.Number:
                    var text = ((JsonNumber)token).ToString();
                    if (Type == typeof(int) && int.TryParse(text, out int intVal)) return intVal;
                    else if (Type == typeof(long) && long.TryParse(text, out long longVal)) return longVal;
                    else if (Type == typeof(float) && float.TryParse(text, out float floatVal)) return floatVal;
                    else if (Type == typeof(double) && double.TryParse(text, out double doubleVal)) return doubleVal;
                    else if (Type == typeof(decimal) && decimal.TryParse(text, out decimal decimalVal)) return decimalVal;
                    else if (Type == typeof(byte) && byte.TryParse(text, out byte byteVal)) return byteVal;
                    else if (Type == typeof(sbyte) && sbyte.TryParse(text, out sbyte sbyteVal)) return sbyteVal;
                    else if (Type == typeof(short) && short.TryParse(text, out short shrotVal)) return shrotVal;
                    else if (Type == typeof(ushort) && ushort.TryParse(text, out ushort ushortVal)) return ushortVal;
                    else if (Type == typeof(uint) && uint.TryParse(text, out uint uintVal)) return uintVal;
                    else if (Type == typeof(ulong) && ulong.TryParse(text, out ulong ulongVal)) return ulongVal;
                    break;
                case JsonValueType.Null:
                    if (Type == typeof(DBNull)) return DBNull.Value;
                    break;
            }
            throw new JsonException($"无法从{token.ValueType}转换为{Type},反序列化{Type}失败");
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
            return Throw(token.ValueType);
        }

        private object GetChar(JsonToken token)
        {
            if (token is JsonString stringToken)
            {
                if (char.TryParse(stringToken.Value, out char value))
                    return value;
                Throw(token.ValueType, stringToken.Value);
            }
            return Throw(token.ValueType);
        }

        private object GetBoolean(JsonToken token)
        {
            if (token is JsonBoolean jsonBoolean)
            {
                return jsonBoolean.Value;
            }
            return Throw(token.ValueType);
        }

        private object GetByte(JsonToken token)
        {
            if (token is JsonNumber jsonNumber)
            {
                if (jsonNumber.TryGetByte(out byte value))
                    return value;
                Throw(token.ValueType, jsonNumber.ToString());
            }
            return Throw(token.ValueType);
        }

        private object GetSByte(JsonToken token)
        {
            if (token is JsonNumber jsonNumber)
            {
                if (jsonNumber.TryGetSByte(out sbyte value))
                    return value;
                Throw(token.ValueType, jsonNumber.ToString());
            }
            return Throw(token.ValueType);
        }

        private object GetInt(JsonToken token)
        {
            if (token is JsonNumber jsonNumber)
            {
                if (jsonNumber.TryGetInt(out int value))
                    return value;
                Throw(token.ValueType, jsonNumber.ToString());
            }
            return Throw(token.ValueType);
        }

        private object GetShort(JsonToken token)
        {
            if (token is JsonNumber jsonNumber)
            {
                if (jsonNumber.TryGetShort(out short value))
                    return value;
                Throw(token.ValueType, jsonNumber.ToString());
            }
            return Throw(token.ValueType);
        }

        private object GetUShort(JsonToken token)
        {
            if (token is JsonNumber jsonNumber)
            {
                if (jsonNumber.TryGetUShort(out ushort value))
                    return value;
                Throw(token.ValueType, jsonNumber.ToString());
            }
            return Throw(token.ValueType);
        }

        private object GetUInt(JsonToken token)
        {
            if (token is JsonNumber jsonNumber)
            {
                if (jsonNumber.TryGetUInt(out uint value))
                    return value;
                Throw(token.ValueType, jsonNumber.ToString());
            }
            return Throw(token.ValueType);
        }

        private object GetLong(JsonToken token)
        {
            if (token is JsonNumber jsonNumber)
            {
                if (jsonNumber.TryGetLong(out long value))
                    return value;
                Throw(token.ValueType, jsonNumber.ToString());
            }
            return Throw(token.ValueType);
        }

        private object GetULong(JsonToken token)
        {
            if (token is JsonNumber jsonNumber)
            {
                if (jsonNumber.TryGetULong(out ulong value))
                    return value;
                Throw(token.ValueType, jsonNumber.ToString());
            }
            return Throw(token.ValueType);
        }

        private object GetFloat(JsonToken token)
        {
            if (token is JsonNumber jsonNumber)
            {
                if (jsonNumber.TryGetFloat(out float value))
                    return value;
                Throw(token.ValueType, jsonNumber.ToString());
            }
            return Throw(token.ValueType);
        }

        private object GetDouble(JsonToken token)
        {
            if (token is JsonNumber jsonNumber)
            {
                if (jsonNumber.TryGetDouble(out double value))
                    return value;
                Throw(token.ValueType, jsonNumber.ToString());
            }
            return Throw(token.ValueType);
        }

        private object GetDecimal(JsonToken token)
        {
            if (token is JsonNumber jsonNumber)
            {
                if (jsonNumber.TryGetDecimal(out decimal value))
                    return value;
                Throw(token.ValueType, jsonNumber.ToString());
            }
            return Throw(token.ValueType);
        }

        private object GetGuid(JsonToken token)
        {
            if (token is JsonString jsonString)
            {
                if (Guid.TryParse(jsonString.Value, out Guid value))
                    return value;
                Throw(token.ValueType, jsonString.Value);
            }
            return Throw(token.ValueType);
        }

        private object GetDBNull(JsonToken token)
        {
            if (token.ValueType == JsonValueType.Null)
            {
                return DBNull.Value;
            }
            return Throw(token.ValueType);
        }

        private object GetDateTime(JsonToken token)
        {
            if (token is JsonString jsonString)
            {
                if (DateTime.TryParse(jsonString.Value, out DateTime value))
                    return value;
                Throw(token.ValueType, jsonString.Value);
            }
            return Throw(token.ValueType);
        }

        private object Throw(JsonValueType valueType, string value = null)
        {
            throw new JsonException($"无法将{valueType}{(value != null ? $":{value}" : "")}转换为{Type},反序列化{Type}失败");
        }

        #endregion

        public override void ToWriter(JsonWriter writer, object value, JsonOption option)
        {
            //switch (_typeCode)
            //{
            //    case TypeCode.Int32: writer.WriteInt((int)value); break;
            //    case TypeCode.UInt32: writer.WriteUInt((uint)value); break;
            //    case TypeCode.Byte: writer.WriteInt((byte)value); break;
            //    case TypeCode.SByte: writer.WriteInt((sbyte)value); break;
            //    case TypeCode.Int16: writer.WriteInt((short)value); break;
            //    case TypeCode.UInt16: writer.WriteInt((ushort)value); break;
            //    case TypeCode.Int64: writer.WriteLong((long)value); break;
            //    case TypeCode.UInt64: writer.WriteULong((ulong)value); break;
            //    case TypeCode.Char: writer.WriteChar((char)value); break;
            //    case TypeCode.Boolean: writer.WriteBoolean((bool)value); break;
            //    case TypeCode.Single: writer.WriteFloat((float)value); break;
            //    case TypeCode.Double: writer.WriteDouble((double)value); break;
            //    case TypeCode.Decimal: writer.WriteDecimal((decimal)value); break;
            //    case TypeCode.DBNull: writer.WriteNull(); break;
            //    default:
            //        if (value is Guid guidVal) writer.WriteGuid(guidVal);
            //        else throw new JsonException($"使用{nameof(ValueConverter)}序列化{value.GetType()}失败，不支持的类型");
            //        break;
            //}

            if (value is int intVal)
                writer.WriteInt(intVal);
            else if (value is long longVal)
                writer.WriteLong(longVal);
            else if (value is bool boolVal)
                writer.WriteBoolean(boolVal);
            else if (value is float floatVal)
                writer.WriteFloat(floatVal);
            else if (value is double doubleVal)
                writer.WriteDouble(doubleVal);
            else if (value is decimal decimalVal)
                writer.WriteDecimal(decimalVal);
            else if (value is Guid guidVal)
                writer.WriteGuid(guidVal);
            else if (value is uint uintVal)
                writer.WriteUInt(uintVal);
            else if (value is byte byteVal)
                writer.WriteInt(byteVal);
            else if (value is sbyte sbyteVal)
                writer.WriteInt(sbyteVal);
            else if (value is short shortVal)
                writer.WriteInt(shortVal);
            else if (value is ushort ushortVal)
                writer.WriteInt(ushortVal);
            else if (value is ulong ulongVal)
                writer.WriteULong(ulongVal);
            else if (value is char charVal)
                writer.WriteChar(charVal);
            else if (value is DBNull)
                writer.WriteNull();
            else throw new JsonException($"使用{nameof(ValueConverter)}序列化{value.GetType()}失败，不支持的类型");
        }
    }
}
