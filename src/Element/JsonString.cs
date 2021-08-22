using System;

namespace Rapidity.Json
{
    public class JsonString : JsonElement, IEquatable<JsonString>
    {
        private string _value;
        public string Value => _value;
        public override JsonElementType ElementType => JsonElementType.String;

        public JsonString() { }

        public JsonString(string value) => _value = value;

        public bool Equals(JsonString other) => other != null && (this.Value?.Equals(other.Value) ?? false);

        public override bool Equals(object obj) => obj is JsonString jsonString && Equals(jsonString);

        public override int GetHashCode() => _value?.GetHashCode() ?? -1;

        public override string ToString() => _value == null ? "null" : $"\"{_value}\"";

        public override string ToString(JsonOption option) => ToString();

        public static implicit operator JsonString(string value) => new JsonString(value);
        public static implicit operator string(JsonString value) => value.Value;

    }
}