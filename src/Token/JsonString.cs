using System;

namespace Rapidity.Json
{
    public class JsonString : JsonToken, IEquatable<JsonString>
    {
        private string _value;
        public string Value => _value;
        public override JsonValueType ValueType => JsonValueType.String;

        #region 构造函数 各种重载

        public JsonString() { }

        public JsonString(string value) => _value = value;

        public JsonString(char[] value) => _value = new string(value);

        public JsonString(int value) => _value = value.ToString();

        public JsonString(short value) => _value = value.ToString();

        public JsonString(long value) => _value = value.ToString();

        public JsonString(float value) => _value = value.ToString();

        public JsonString(double value) => _value = value.ToString();

        public JsonString(decimal value) => _value = value.ToString();

        public JsonString(Guid value) => _value = value.ToString();

        public JsonString(DateTime value) => _value = value.ToString();

        public JsonString(DateTimeOffset value) => _value = value.ToString();

        #endregion
        public bool Equals(JsonString other) => other != null && this.Value.Equals(other.Value);

        public override object To(Type type)
        {
            throw new NotImplementedException();
        }
    }
}