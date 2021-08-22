using System;

namespace Rapidity.Json
{
    public class JsonBoolean : JsonElement, IEquatable<JsonBoolean>
    {
        private bool _value;
        public bool Value => _value;

        public override JsonElementType ElementType => JsonElementType.Boolean;

        public JsonBoolean() { }

        public JsonBoolean(bool value) => _value = value;

        public bool Equals(JsonBoolean other) => Value.Equals(other.Value);

        public override bool Equals(object obj) => obj is JsonBoolean jsonBoolean && Equals(jsonBoolean);

        public override int GetHashCode() => _value.GetHashCode();

        public static implicit operator JsonBoolean(bool value) => new JsonBoolean(value);
        public static implicit operator bool(JsonBoolean value) => value.Value;

        public static bool operator ==(JsonBoolean value1, JsonBoolean value2) => value1.Value == value2.Value;
        public static bool operator !=(JsonBoolean value1, JsonBoolean value2) => value1.Value != value2.Value;
    }
}