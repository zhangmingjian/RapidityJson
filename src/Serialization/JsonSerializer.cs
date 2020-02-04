using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rapidity.Json.Serialization
{
    public class JsonSerializer
    {
        public object Deserialize(JsonReader reader, Type type)
        {
            var provider = new TypeDescriptorProvider();
            var desc = provider.GetDescriptor(type);
            switch (desc.TypeKind)
            {
                case TypeKind.Object: return DeserializeObject(reader, (ObjectDescriptor)desc);
                case TypeKind.Value: return DeserializeValue(reader, type);
            }
            return null;
        }

        private object DeserializeObject(JsonReader reader, ObjectDescriptor desc)
        {
            object instance = null;
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject: instance = desc.CreateMethod(); break;
                    case JsonTokenType.EndObject: return instance;
                    case JsonTokenType.PropertyName:
                        var property = desc.PropertyDescriptors.FirstOrDefault(x => x.JsonAlias.Equals(reader.Value, StringComparison.CurrentCultureIgnoreCase));
                        if (property != null)
                        {
                            property.SetValueMethod(instance, Deserialize(reader, property.MemberType));
                        }
                        break;
                }
            }
            return instance;
        }

        private object DeserializeValue(JsonReader reader, Type type)
        {
            reader.Read();
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return reader.GetBoolean();
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    return reader.GetInt();
                case TypeCode.Int64:
                    return reader.GetLong();
                case TypeCode.Single:
                    return reader.GetFloat();
                case TypeCode.Double: return reader.GetDouble();
                case TypeCode.Decimal: return reader.GetDecimal();
                case TypeCode.String: return reader.GetString();
                case TypeCode.DateTime: return reader.GetDateTime();
                default:
                    if (type == typeof(Guid))
                        return reader.GetGuid();
                    if (type.IsValueType)
                        return Activator.CreateInstance(type);
                    else return default;
            }
        }

        public string Serialize(object obj)
        {
            return null;
        }

        public T Deserialize<T>(JsonReader reader)
        {
            return (T)Deserialize(reader, typeof(T));
        }
    }
}