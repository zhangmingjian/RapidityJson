using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rapidity.Json.Reflect
{
    public interface IMemberAccessor
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
