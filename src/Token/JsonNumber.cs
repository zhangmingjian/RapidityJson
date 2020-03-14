using System;

namespace Rapidity.Json
{
    public class JsonNumber : JsonToken, IEquatable<JsonNumber>
    {
        private string _value;

        public override JsonValueType ValueType => JsonValueType.Number;

        public JsonNumber() => _value = "0";

        public JsonNumber(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (!double.TryParse(value, out double val))
                throw new JsonException($"无效的JSON Number值：{value}");
            _value = value;
        }

        public JsonNumber(int value) => _value = value.ToString();

        public JsonNumber(short value) => _value = value.ToString();

        public JsonNumber(long value) => _value = value.ToString();

        public JsonNumber(ulong value) => _value = value.ToString();

        public JsonNumber(float value) => _value = value.ToString();

        public JsonNumber(double value) => _value = value.ToString();

        public JsonNumber(decimal value) => _value = value.ToString();

        public bool TryGetByte(out byte value) => byte.TryParse(_value, out value);

        public bool TryGetSByte(out sbyte value) => sbyte.TryParse(_value, out value);

        public bool TryGetInt(out int value) => int.TryParse(_value, out value);

        public bool TryGetUInt(out uint value) => uint.TryParse(_value, out value);

        public bool TryGetShort(out short value) => short.TryParse(_value, out value);

        public bool TryGetUShort(out ushort value) => ushort.TryParse(_value, out value);

        public bool TryGetLong(out long value) => long.TryParse(_value, out value);

        public bool TryGetULong(out ulong value) => ulong.TryParse(_value, out value);

        public bool TryGetFloat(out float value) => float.TryParse(_value, out value);

        public bool TryGetDouble(out double value) => double.TryParse(_value, out value);

        public bool TryGetDecimal(out decimal value) => decimal.TryParse(_value, out value);

        public override string ToString() => _value;

        public bool Equals(JsonNumber other) => other != null && this._value.Equals(other._value);

        public override bool Equals(object obj) => obj is JsonNumber jsonNumber && Equals(jsonNumber);

        public override int GetHashCode() => _value.GetHashCode();
    }
}
