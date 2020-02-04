using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rapidity.Json.Serialization
{
    internal class TypeDescriptor
    {
        public TypeKind TypeKind { get; set; }
        public Func<object> CreateMethod { get; set; }
    }

    internal class ValueDescriptor : TypeDescriptor
    {
        public ValueDescriptor()
        {
            TypeKind = TypeKind.Value;
        }
    }

    internal class ObjectDescriptor : TypeDescriptor
    {
        public ObjectDescriptor()
        {
            TypeKind = TypeKind.Object;
        }

        public List<PropertyDescriptor> PropertyDescriptors { get; set; }
    }

    internal class PropertyDescriptor
    {
        public string Name => MemberInfo.Name;

        public string JsonAlias { get; set; }

        public MemberInfo MemberInfo { get; set; }

        public Type MemberType { get; set; }

        public Func<object, object> GetValueMethod { get; set; }

        public Action<object, object> SetValueMethod { get; set; }
    }

    internal class ListDescriptor : TypeDescriptor
    {
        public Action<object, object> AddMethod { get; set; }
    }

    internal class ArrayDescriptor : ListDescriptor
    {
        public Func<object, object> ToArrayMethod { get; set; }
    }


    internal enum TypeKind : byte
    {
        Value,
        Object,
        Array,
        List,
        Dictionary
    }
}
