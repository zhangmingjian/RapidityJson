using System;

namespace Rapidity.Json
{
    public class JsonString : JsonToken, IEquatable<JsonString>
    {
        private string _value;
        public string Value => _value;
        public override JsonValueType ValueType => JsonValueType.String;

        public JsonString(string value) => _value = value;     

        public bool Equals(JsonString other) => other != null && this.Value.Equals(other.Value);

        public override bool Equals(object obj) => obj is JsonString jsonString && Equals(jsonString);

        public override int GetHashCode() => _value.GetHashCode();
    }
}