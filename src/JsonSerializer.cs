using Rapidity.Json.Converters;
using System;
using System.IO;

namespace Rapidity.Json
{
    public class JsonSerializer
    {
        private readonly JsonOption _option;

        public JsonSerializer() : this(JsonOption.Defalut)
        {
        }

        public JsonSerializer(JsonOption option)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));
            this._option = option;
        }

        public object Deserialize(JsonReader reader, Type type)
        {
            var convert = _option.ConverterFactory.Build(type);
            return convert.FromReader(reader, _option);
        }


        public T Deserialize<T>(JsonReader reader)
        {
            return (T)Deserialize(reader, typeof(T));
        }

        public object Deserialize(JsonToken token, Type type)
        {
            var provider = new DefaultTypeConverterProvider();
            var convert = provider.Build(type);
            return convert.FromToken(token, _option);
        }

        public T Deserialize<T>(JsonToken token)
        {
            return (T)Deserialize(token, typeof(T));
        }

        public string Serialize(object obj)
        {
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
            convert.WriteTo(writer, obj, _option);
        }
    }
}