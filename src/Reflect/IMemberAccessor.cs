using System;
using System.Reflection;

namespace Rapidity.Json.Reflect
{
    internal interface IMemberAccessor
    {
        Type MemberType { get; }

        MemberInfo MemberInfo { get; }

        string Name { get; }

        bool CanGet { get; }

        bool CanSet { get; }

        object GetValue(object instance);

        void SetValue(object instance, object value);
    }
}
