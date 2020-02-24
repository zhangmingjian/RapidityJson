using System;

namespace Rapidity.Json
{
    public class JsonNull : JsonToken, IEquatable<JsonNull>
    {
        public override JsonValueType ValueType => JsonValueType.Null;

        public bool Equals(JsonNull other) => other != null;

        public override bool Equals(object obj) => obj is JsonNull;

        public override int GetHashCode() => -1;
    }
}