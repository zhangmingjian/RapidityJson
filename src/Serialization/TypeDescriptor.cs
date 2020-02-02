using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rapidity.Json.Serialization
{
    internal abstract class TypeDescriptor
    {
        Type Type { get; }
        public virtual TypeKind TypeKind { get; }
        Func<object> CreateFunc { get; set; }
    }

    internal class ObjectDescriptor : TypeDescriptor
    {
        public override TypeKind TypeKind => TypeKind.Object;

        public Func<object> CreateFunc { get; set; }
    }

    internal class PropertyDescriptor
    {
        public string Name { get; }

        public string JsonAlias { get; }

        public TypeDescriptor TypeDescriptor { get; }

        public PropertyInfo PropertyInfo { get; set; }
    }

    internal enum TypeKind : byte
    {
        Unknown,
        Object,
        Value,
        Array,
        Dictionary
    }
}
