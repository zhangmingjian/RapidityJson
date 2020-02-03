using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rapidity.Json.Serialization
{
    internal class TypeDescriptor
    {
        public TypeKind TypeKind { get; set; }
        public Func<object> Create { get; set; }
    }

    internal class PropertyDescriptor
    {
        public string Name => MemberInfo.Name;

        public string JsonAlias { get; }

        public MemberInfo MemberInfo { get; set; }
    }

    internal class ListDesciptor : TypeDescriptor
    {
        public Action<object, object> Add { get; set; }
    }

    internal enum TypeKind : byte
    {
        Object,
        Value,
        Array,
        List,
        Dictionary
    }
}
