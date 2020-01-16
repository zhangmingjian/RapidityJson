using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json
{
    public class JsonNull : JsonToken, IEquatable<JsonNull>
    {
        public override JsonValueType ValueType => JsonValueType.Null;

        public bool Equals(JsonNull other) => other != null;

        public override int GetHashCode() => -1;

        public override object To(Type type)
        {
            return default;
        }
    }
}