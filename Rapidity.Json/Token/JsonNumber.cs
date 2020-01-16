using System;
using System.Globalization;

namespace Rapidity.Json
{
    public class JsonNumber : JsonToken, IEquatable<JsonNumber>
    {
        private double _value;

        public override JsonValueType ValueType => JsonValueType.Number;

        public JsonNumber() { }

        public JsonNumber(string value) => this._value = double.Parse(value, CultureInfo.InvariantCulture);

        public JsonNumber(int value) => _value = value;

        public JsonNumber(short value) => _value = value;

        public JsonNumber(long value) => _value = value;
        public JsonNumber(ulong value) => _value = value;

        public JsonNumber(float value) => _value = value;

        public JsonNumber(double value) => _value = value;

        public JsonNumber(decimal value) => _value = (double)value;

        public int GetInt()
        {
            checked { return (int)_value; }
        }

        public uint GetUInt()
        {
            checked { return (uint)_value; }
        }

        public short GetShort()
        {
            checked { return (short)_value; }
        }

        public ushort GetUShort()
        {
            checked { return (ushort)_value; }
        }

        public long GetLong()
        {
            checked { return (long)_value; }
        }

        public ulong GetULong()
        {
            checked { return (ulong)_value; }
        }

        public float GetFloat()
        {
            checked { return (float)_value; }
        }

        public double GetDouble() => _value;

        public decimal GetDecimal()
        {
            checked { return (decimal)_value; }
        }

        public bool Equals(JsonNumber other) => other != null && this._value.Equals(other._value);

        public override bool Equals(object obj) => obj is JsonNumber jsonNumber && Equals(jsonNumber);

        public override string ToString() => _value.ToString();

        public override int GetHashCode() => _value.GetHashCode();

        public override object To(Type type)
        {
            throw new NotImplementedException();
        }
    }
}
