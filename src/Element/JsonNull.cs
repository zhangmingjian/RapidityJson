using System;

namespace Rapidity.Json
{
    public class JsonNull : JsonElement, IEquatable<JsonNull>
    {
        public override JsonElementType ElementType => JsonElementType.Null;

        public bool Equals(JsonNull other) => other != null;

        public override bool Equals(object obj) => obj is JsonNull;

        public override int GetHashCode() => -1;
    }
}