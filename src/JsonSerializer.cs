using Rapidity.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rapidity.Json
{
    public class JsonSerializer
    {
        public object Deserialize(JsonReader reader, Type type)
        {
            var provider = new DefaultTypeConverterProvider();
            var convert = provider.Build(type);
            return convert.FromReader(reader);
        }

        public T Deserialize<T>(JsonReader reader)
        {
            return (T)Deserialize(reader, typeof(T));
        }

        public string Serialize(object obj)
        {
            if (obj == null) throw new JsonException("序列化对象的值不能为Null");
            using (var sw = new StringWriter())
            using (var writer = new JsonWriter(sw))
            {
                Serialize(writer, obj);
                return sw.ToString();
            }
        }

        public void Serialize(JsonWriter writer, object obj)
        {
            if (obj == null)
            {
                writer.WriteNull();
                return;
            }
            var provider = new DefaultTypeConverterProvider();
            var convert = provider.Build(obj.GetType());
            convert.WriteTo(writer, obj);
        }
    }
}