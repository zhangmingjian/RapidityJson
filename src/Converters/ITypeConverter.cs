using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidity.Json.Converters
{
    public interface ITypeConverter
    {
        Type Type { get; }

        object FromReader(JsonReader reader, JsonOption option);

        object FromToken(JsonToken token, JsonOption option);

        void ToWriter(JsonWriter writer, object obj, JsonOption option);
    }
}
