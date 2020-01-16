using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json
{
    public class JsonBoolean : JsonToken, IEquatable<JsonBoolean>
    {
        public bool Value { get; private set; }

        public override JsonValueType ValueType => JsonValueType.Boolean;

        public JsonBoolean() { }

        public JsonBoolean(bool value)
        {
            this.Value = value;
        }

        public JsonBoolean(string value)
        {
            this.Value = bool.Parse(value);
        }

        public bool Equals(JsonBoolean other)
        {
            return Value.Equals(other.Value);
        }

        public override object To(Type type)
        {
            throw new NotImplementedException();
        }
    }
}