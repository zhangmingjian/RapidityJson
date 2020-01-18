using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Serialization
{
    public abstract class JsonSerializer
    {
        public abstract object Deserialize(JsonReader reader, Type type);

        public T Deserialize<T>(JsonReader reader)
        {
            return (T)Deserialize(reader, typeof(T));
        }

        public abstract string Serialize(object obj);
    }
}