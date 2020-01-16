using System;

namespace Rapidity.Json
{
    public class JsonString : JsonToken, IEquatable<JsonString>
    {
        public string Value { get; private set; }
        public override JsonValueType ValueType => JsonValueType.String;

        #region 构造函数 各种重载
        public JsonString() { }

        public JsonString(string value)
        {
            this.Value = value;
        }
        public JsonString(char[] value)
        {
            this.Value = new string(value);
        }

        public JsonString(int value)
        {
            this.Value = value.ToString();
        }

        public JsonString(long value)
        {
            this.Value = value.ToString();
        }

        public JsonString(float value)
        {
            this.Value = value.ToString();
        }

        public JsonString(double value)
        {
            this.Value = value.ToString();
        }

        public JsonString(decimal value)
        {
            this.Value = value.ToString();
        }

        public JsonString(Guid value)
        {
            this.Value = value.ToString();
        }

        public JsonString(DateTime value)
        {
            this.Value = value.ToString();
        }

        public JsonString(DateTimeOffset value)
        {
            this.Value = value.ToString();
        }
        #endregion
        public bool Equals(JsonString other) => other != null && this.Value.Equals(other.Value);

        public override object To(Type type)
        {
            throw new NotImplementedException();
        }
    }
}