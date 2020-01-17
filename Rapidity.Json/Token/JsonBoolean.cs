using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json
{
    public class JsonBoolean : JsonToken, IEquatable<JsonBoolean>
    {
        private bool _value;
        public bool Value => _value;

        public override JsonValueType ValueType => JsonValueType.Boolean;

        public JsonBoolean() { }

        public JsonBoolean(bool value) => _value = value;

        public JsonBoolean(string value) => _value = bool.Parse(value);

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