using System;
using System.IO;

namespace Rapidity.Json
{
    public class JsonSerializer
    {
        public JsonOption Option { get; }

        public JsonSerializer() : this(new JsonOption { SkipValidated = true })
        {
        }

        public JsonSerializer(JsonOption option)
        {
            Option = option ?? throw new ArgumentNullException(nameof(option));
        }

        public object Deserialize(JsonReader reader, Type type)
        {
            var convert = Option.ConverterProvider.Build(type);
            return convert.FromReader(reader, Option);
        }


        public T Deserialize<T>(JsonReader reader)
        {
            return (T)Deserialize(reader, typeof(T));
        }

        public object Deserialize(JsonElement element, Type type)
        {
            var convert = Option.ConverterProvider.Build(type);
            return convert.FromElement(element, Option);
        }


        public T Deserialize<T>(JsonElement element)
        {
            return (T)Deserialize(element, typeof(T));
        }

        public string Serialize(object obj)
        {
            using (var sw = new StringWriter())
            using (var writer = new JsonWriter(sw, Option))
            {
                Serialize(writer, obj);
                sw.Flush();
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
            var convert = Option.ConverterProvider.Build(obj.GetType());
            Option.LoopReferenceChecker.PushRootObject(obj);
            convert.ToWriter(writer, obj, Option);
        }
    }
}