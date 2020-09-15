using System;

namespace Rapidity.Json.Converters
{
    internal class ValueConverter : ITypeConverter, IConverterCreator
    {
        private TypeCode _typeCode;
        public Type Type { get; }
        public ValueConverter(Type type)
        {
            Type = type;
            _typeCode = Type.GetTypeCode(type);
        }

        public bool CanConvert(Type type)
        {
            //The primitive types are Boolean, Byte, SByte, Int16, UInt16, Int32, UInt32, 
            //Int64, UInt64, IntPtr, UIntPtr, Char, Double, and Single.
            return type.IsPrimitive
                     || type == typeof(decimal)
                     || type == typeof(Guid)
                     || type == typeof(DBNull);
        }

        public ITypeConverter Create(Type type)
        {
            return new ValueConverter(type);
        }

        public object FromReader(JsonReader reader, JsonOption option)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.None:
                    reader.Read();
                    return FromReader(reader, option);
                case JsonTokenType.True:
                    if (_typeCode == TypeCode.Boolean) return true;
                    break;
                case JsonTokenType.False:
                    if (_typeCode == TypeCode.Boolean) return false;
                    break;
                case JsonTokenType.String:
                    switch (_typeCode)
                    {
                        case TypeCode.Char:
                            if (char.TryParse(reader.Text, out char charVal)) return charVal;
                            break;
                        case TypeCode.Single:
                            if (reader.Text == JsonConstants.NaN) return float.NaN;
                            if (reader.Text == JsonConstants.NegativeInfinity) return float.NegativeInfinity;
                            if (reader.Text == JsonConstants.PositiveInfinity) return float.PositiveInfinity;
                            break;
                        case TypeCode.Double:
                            if (reader.Text == JsonConstants.NaN) return double.NaN;
                            if (reader.Text == JsonConstants.NegativeInfinity) return double.NegativeInfinity;
                            if (reader.Text == JsonConstants.PositiveInfinity) return double.PositiveInfinity;
                            break;
                        default:
                            if (Type == typeof(Guid) && Guid.TryParse(reader.Text, out Guid guidVal)) return guidVal;
                            break;
                    }
                    break;
                case JsonTokenType.Number:
                    switch (_typeCode)
                    {
                        case TypeCode.Int32:
                            if (int.TryParse(reader.Text, out int intVal)) return intVal;
                            break;
                        case TypeCode.Int64:
                            if (long.TryParse(reader.Text, out long longVal)) return longVal;
                            break;
                        case TypeCode.Single:
                            if (float.TryParse(reader.Text, out float floatVal)) return floatVal;
                            break;
                        case TypeCode.Double:
                            if (double.TryParse(reader.Text, out double doubleVal)) return doubleVal;
                            break;
                        case TypeCode.Decimal:
                            if (decimal.TryParse(reader.Text, out decimal decimalVal)) return decimalVal;
                            break;
                        case TypeCode.Byte:
                            if (byte.TryParse(reader.Text, out byte byteVal)) return byteVal;
                            break;
                        case TypeCode.SByte:
                            if (sbyte.TryParse(reader.Text, out sbyte sbyteVal)) return sbyteVal;
                            break;
                        case TypeCode.Int16:
                            if (short.TryParse(reader.Text, out short shrotVal)) return shrotVal;
                            break;
                        case TypeCode.UInt16:
                            if (ushort.TryParse(reader.Text, out ushort ushortVal)) return ushortVal;
                            break;
                        case TypeCode.UInt32:
                            if (uint.TryParse(reader.Text, out uint uintVal)) return uintVal;
                            break;
                        case TypeCode.UInt64:
                            if (ulong.TryParse(reader.Text, out ulong ulongVal)) return ulongVal;
                            break;
                    }
                    break;
                case JsonTokenType.Null:
                    if (Type == typeof(DBNull)) return DBNull.Value;
                    break;
            }
            throw new JsonException($"无效的JSON Token:{reader.TokenType},序列化对象:{Type}", reader.Line, reader.Position);
        }

        public object FromElement(JsonElement element, JsonOption option)
        {
            switch (element.ElementType)
            {
                case JsonElementType.Boolean:
                    if (_typeCode == TypeCode.Boolean) return ((JsonBoolean)element).Value;
                    break;
                case JsonElementType.String:
                    var strToken = (JsonString)element;
                    switch (_typeCode)
                    {
                        case TypeCode.Char:
                            if (char.TryParse(strToken.Value, out char charVal)) return charVal;
                            break;
                        case TypeCode.Single:
                            if (strToken.Value == JsonConstants.NaN) return float.NaN;
                            if (strToken.Value == JsonConstants.NegativeInfinity) return float.NegativeInfinity;
                            if (strToken.Value == JsonConstants.PositiveInfinity) return float.PositiveInfinity;
                            break;
                        case TypeCode.Double:
                            if (strToken.Value == JsonConstants.NaN) return double.NaN;
                            if (strToken.Value == JsonConstants.NegativeInfinity) return double.NegativeInfinity;
                            if (strToken.Value == JsonConstants.PositiveInfinity) return double.PositiveInfinity;
                            break;
                        default:
                            if (Type == typeof(Guid) && Guid.TryParse(strToken.Value, out Guid guidVal)) return guidVal;
                            break;
                    }
                    break;
                case JsonElementType.Number:
                    var text = ((JsonNumber)element).ToString();
                    switch (_typeCode)
                    {
                        case TypeCode.Int32:
                            if (int.TryParse(text, out int intVal)) return intVal;
                            break;
                        case TypeCode.Int64:
                            if (long.TryParse(text, out long longVal)) return longVal;
                            break;
                        case TypeCode.Single:
                            if (float.TryParse(text, out float floatVal)) return floatVal;
                            break;
                        case TypeCode.Double:
                            if (double.TryParse(text, out double doubleVal)) return doubleVal;
                            break;
                        case TypeCode.Decimal:
                            if (decimal.TryParse(text, out decimal decimalVal)) return decimalVal;
                            break;
                        case TypeCode.Byte:
                            if (byte.TryParse(text, out byte byteVal)) return byteVal;
                            break;
                        case TypeCode.SByte:
                            if (sbyte.TryParse(text, out sbyte sbyteVal)) return sbyteVal;
                            break;
                        case TypeCode.Int16:
                            if (short.TryParse(text, out short shrotVal)) return shrotVal;
                            break;
                        case TypeCode.UInt16:
                            if (ushort.TryParse(text, out ushort ushortVal)) return ushortVal;
                            break;
                        case TypeCode.UInt32:
                            if (uint.TryParse(text, out uint uintVal)) return uintVal;
                            break;
                        case TypeCode.UInt64:
                            if (ulong.TryParse(text, out ulong ulongVal)) return ulongVal;
                            break;
                    }
                    break;
                case JsonElementType.Null:
                    if (Type == typeof(DBNull)) return DBNull.Value;
                    break;
            }
            throw new JsonException($"无法从{element.ElementType}转换为{Type},反序列化{Type}失败");
        }

        public void ToWriter(JsonWriter writer, object value, JsonOption option)
        {
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
