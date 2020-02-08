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
            using (var write = new JsonWriter(sw))
            {
                //Serialize(write, obj);
                return sw.ToString();
            }
        }

        //public void Serialize(JsonWriter writer, object obj)
        //{
        //    if (obj == null)
        //    {
        //        writer.WriteNull();
        //        return;
        //    }
        //    var desc = TypeDescriptor.Create(obj.GetType());
        //    switch (desc.TypeKind)
        //    {
        //        case TypeKind.Object: WriteObject(writer, obj, (ObjectDescriptor)desc); break;
        //        case TypeKind.Value: writer.WriteValue(obj); break;
        //        case TypeKind.List: WriteList(writer, obj, (EnumerableDescriptor)desc); break;
        //        case TypeKind.Dictionary: WriteDictionary(writer, obj, (DictionaryDescriptor)desc); break;
        //    }
        //}

        //#region write
        //private void WriteObject(JsonWriter writer, object obj, ObjectDescriptor descriptor)
        //{
        //    writer.WriteStartObject();
        //    foreach (var member in descriptor.MemberDefinitions)
        //    {
        //        writer.WritePropertyName(member.JsonProperty);
        //        var value = member.GetValue(obj);
        //        Serialize(writer, value);
        //    }
        //    writer.WriteEndObject();
        //}

        //private void WriteList(JsonWriter writer, object list, EnumerableDescriptor descriptor)
        //{
        //    writer.WriteStartArray();
        //    var enumer = descriptor.GetEnumerator(list);
        //    while (enumer.MoveNext())
        //    {
        //        Serialize(writer, enumer.Current);
        //    }
        //    writer.WriteEndArray();
        //}

        //private void WriteDictionary(JsonWriter writer, object dic, DictionaryDescriptor descriptor)
        //{
        //    writer.WriteStartObject();
        //    var keys = descriptor.GetKeys(dic);
        //    while (keys.MoveNext())
        //    {
        //        writer.WritePropertyName(keys.Current.ToString());
        //        var value = descriptor.GetValue(dic, keys.Current);
        //        Serialize(writer, value);
        //    }
        //    writer.WriteEndObject();
        //}

        //#endregion


    }
}